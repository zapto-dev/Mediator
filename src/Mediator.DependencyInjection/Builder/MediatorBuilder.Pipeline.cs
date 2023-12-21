using System;
using System.Linq;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public partial class MediatorBuilder : IMediatorBuilder
{
	public IMediatorBuilder AddPipelineBehavior<TRequest, TResponse>(IPipelineBehavior<TRequest, TResponse> behavior) where TRequest : notnull
	{
		_services.AddSingleton(behavior);
		return this;
	}

	public IMediatorBuilder AddPipelineBehavior(Type behaviorType)
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
				behaviorType);
		}

		return this;
	}

	public IMediatorBuilder AddPipelineBehavior(Type requestType, Type? responseType, Type behaviorType)
	{
		if (requestType.IsGenericType || responseType is null || responseType.IsGenericTypeDefinition)
		{
			throw new NotSupportedException("Generic pipeline behaviors are not supported.");
		}

		_services.AddTransient(typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType), behaviorType);
		return this;
	}

	public IMediatorBuilder AddPipelineBehavior<TRequest, TResponse, TBehavior>() where TRequest : notnull where TBehavior : class, IPipelineBehavior<TRequest, TResponse>
	{
		_services.AddSingleton<IPipelineBehavior<TRequest, TResponse>, TBehavior>();
		return this;
	}

	public IMediatorBuilder AddStreamPipelineBehavior<TRequest, TResponse>(IStreamPipelineBehavior<TRequest, TResponse> behavior) where TRequest : IStreamRequest<TResponse>
	{
		_services.AddSingleton(behavior);
		return this;
	}

	public IMediatorBuilder AddStreamPipelineBehavior(Type behaviorType)
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
				behaviorType);
		}

		return this;
	}

	public IMediatorBuilder AddStreamPipelineBehavior(Type requestType, Type? responseType, Type behaviorType)
	{
		if (requestType.IsGenericType || responseType is null || responseType.IsGenericTypeDefinition)
		{
			throw new NotSupportedException("Generic pipeline behaviors are not supported.");
		}

		_services.AddTransient(typeof(IStreamPipelineBehavior<,>).MakeGenericType(requestType, responseType), behaviorType);
		return this;
	}

	public IMediatorBuilder AddStreamPipelineBehavior<TRequest, TResponse, TBehavior>() where TRequest : IStreamRequest<TResponse> where TBehavior : class, IStreamPipelineBehavior<TRequest, TResponse>
	{
		_services.AddSingleton<IStreamPipelineBehavior<TRequest, TResponse>, TBehavior>();
		return this;
	}
}
