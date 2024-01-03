using System;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

public partial class MediatorBuilder : IMediatorBuilder
{
	public IMediatorBuilder AddPipelineBehavior<TRequest, TResponse>(IPipelineBehavior<TRequest, TResponse> behavior) where TRequest : notnull
	{
		_services.AddSingleton(behavior);
		return this;
	}

	public IMediatorBuilder AddPipelineBehavior(Type behaviorType, RegistrationScope scope = RegistrationScope.Transient)
	{
		var handlers = behaviorType.GetInterfaces()
			.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
			.ToArray();

		foreach (var type in handlers)
		{
			var requestType = type.GetGenericArguments()[0];
			var responseType = type.GetGenericArguments()[1];

			if (requestType.IsGenericType)
			{
				requestType = requestType.GetGenericTypeDefinition();
			}

			AddPipelineBehavior(
				requestType,
				responseType switch
				{
					{ IsGenericParameter: true } => null,
					{ IsGenericType: true } when HasGenericParameter(responseType) => responseType.GetGenericTypeDefinition(),
					_ => responseType
				},
				behaviorType,
				scope);
		}

		return this;
	}

	public IMediatorBuilder AddPipelineBehavior(Type requestType, Type? responseType, Type behaviorType, RegistrationScope scope = RegistrationScope.Transient)
	{
		if (requestType.IsGenericType || responseType is null || responseType.IsGenericTypeDefinition)
		{
			_services.TryAdd(new ServiceDescriptor(behaviorType, behaviorType, GetLifetime(scope)));
			_services.AddSingleton(new GenericPipelineBehaviorRegistration(requestType, responseType, behaviorType));
		}
		else
		{
			_services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType), behaviorType, GetLifetime(scope)));
		}

		return this;
	}

	public IMediatorBuilder AddPipelineBehavior<TRequest, TResponse, TBehavior>(RegistrationScope scope = RegistrationScope.Transient) where TRequest : notnull where TBehavior : class, IPipelineBehavior<TRequest, TResponse>
	{
		_services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<TRequest, TResponse>), typeof(TBehavior), GetLifetime(scope)));
		return this;
	}

	public IMediatorBuilder AddStreamPipelineBehavior<TRequest, TResponse>(IStreamPipelineBehavior<TRequest, TResponse> behavior) where TRequest : IStreamRequest<TResponse>
	{
		_services.AddSingleton(behavior);
		return this;
	}

	public IMediatorBuilder AddStreamPipelineBehavior(Type behaviorType, RegistrationScope scope = RegistrationScope.Transient)
	{
		var handlers = behaviorType.GetInterfaces()
			.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IStreamPipelineBehavior<,>))
			.ToArray();

		foreach (var type in handlers)
		{
			var requestType = type.GetGenericArguments()[0];
			var responseType = type.GetGenericArguments()[1];

			if (requestType.IsGenericType)
			{
				requestType = requestType.GetGenericTypeDefinition();
			}

			AddStreamPipelineBehavior(
				requestType,
				responseType switch
				{
					{ IsGenericParameter: true } => null,
					{ IsGenericType: true } when HasGenericParameter(responseType) => responseType.GetGenericTypeDefinition(),
					_ => responseType
				},
				behaviorType,
				scope);
		}

		return this;
	}

	public IMediatorBuilder AddStreamPipelineBehavior(Type requestType, Type? responseType, Type behaviorType, RegistrationScope scope = RegistrationScope.Transient)
	{
		if (requestType.IsGenericType || responseType is null || responseType.IsGenericTypeDefinition)
		{
			throw new NotSupportedException("Generic pipeline behaviors are not supported.");
		}

		_services.Add(new ServiceDescriptor(typeof(IStreamPipelineBehavior<,>).MakeGenericType(requestType, responseType), behaviorType, GetLifetime(scope)));
		return this;
	}

	public IMediatorBuilder AddStreamPipelineBehavior<TRequest, TResponse, TBehavior>(RegistrationScope scope = RegistrationScope.Transient) where TRequest : IStreamRequest<TResponse> where TBehavior : class, IStreamPipelineBehavior<TRequest, TResponse>
	{
		_services.Add(new ServiceDescriptor(typeof(IStreamPipelineBehavior<TRequest, TResponse>), typeof(TBehavior), GetLifetime(scope)));
		return this;
	}
}
