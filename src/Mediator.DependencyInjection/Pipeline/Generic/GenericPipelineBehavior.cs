using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

internal sealed record GenericPipelineBehaviorRegistration
{
	public GenericPipelineBehaviorRegistration(Type requestType, Type? responseType, Type behaviorType)
	{
		RequestType = requestType;
		ResponseType = responseType;
		BehaviorType = behaviorType;
	}

	public Type RequestType { get; }

	public Type? ResponseType { get; }

	public Type BehaviorType { get; }
}

internal sealed class GenericPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	private readonly List<Type> _handlerTypes;
	private readonly IEnumerable<GenericPipelineBehaviorRegistration> _enumerable;

	public GenericPipelineBehavior(IEnumerable<GenericPipelineBehaviorRegistration> enumerable)
	{
		_enumerable = enumerable;
		_handlerTypes = CreateHandlerTypes();
	}

	public bool IsEmpty => _handlerTypes.Count == 0;

	private List<Type> CreateHandlerTypes()
	{
		var handlerTypes = new List<Type>();

		if (_enumerable is GenericPipelineBehaviorRegistration[] { Length: 0 })
		{
			return handlerTypes;
		}

		var requestType = typeof(TRequest);
		var arguments = requestType.GetGenericArguments();

		if (requestType.IsGenericType)
		{
			requestType = requestType.GetGenericTypeDefinition();
		}

		var responseType = typeof(TResponse);

		if (responseType.IsGenericType)
		{
			responseType = responseType.GetGenericTypeDefinition();
		}

		foreach (var registration in _enumerable)
		{
			if (!registration.RequestType.IsAssignableFrom(requestType) ||
			    registration.ResponseType is not null && !registration.ResponseType.IsAssignableFrom(responseType))
			{
				continue;
			}

			var type = registration.BehaviorType.MakeGenericType(arguments);

			handlerTypes.Add(type);
		}

		handlerTypes.Reverse();

		return handlerTypes;
	}

	public ValueTask<TResponse> Handle(IServiceProvider provider, TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		foreach (var cachedType in _handlerTypes)
		{
			var behavior = (IPipelineBehavior<TRequest, TResponse>)provider.GetRequiredService(cachedType);
			var nextPipeline = next;

			next = () => behavior.Handle(provider, request, nextPipeline, cancellationToken);
		}

		return next();
	}
}