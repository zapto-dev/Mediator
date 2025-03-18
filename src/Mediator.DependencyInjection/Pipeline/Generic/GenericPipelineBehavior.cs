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
			if ((registration.RequestType.IsGenericParameter
				    ? !MatchesGenericParameter(requestType, registration.RequestType)
				    : !registration.RequestType.IsAssignableFrom(requestType)
				) ||
				registration.ResponseType is not null && !registration.ResponseType.IsAssignableFrom(responseType))
			{
				continue;
			}

			Type type;

			if (registration.BehaviorType.IsGenericType)
			{
				var map = new Dictionary<Type, Type?>();
				var genericArguments = registration.BehaviorType.GetGenericArguments();
				var behaviorArguments = new Type[genericArguments.Length];

				foreach (var argument in genericArguments)
				{
					map[argument] = null;
				}

				var isValid = true;

				for (var i = 0; i < genericArguments.Length; i++)
				{
					if (map.TryGetValue(genericArguments[i], out var value) && value is not null)
					{
						behaviorArguments[i] = value;
					}
					else if (MatchesGenericParameter(typeof(TRequest), genericArguments[i], map))
					{
						behaviorArguments[i] = typeof(TRequest);
					}
					else if (MatchesGenericParameter(typeof(TResponse), genericArguments[i], map))
					{
						behaviorArguments[i] = typeof(TResponse);
					}
					else
					{
						isValid = false;
					}
				}

				if (!isValid)
				{
					continue;
				}

				type = registration.BehaviorType.MakeGenericType(behaviorArguments);
			}
			else
			{
				type = registration.BehaviorType;
			}


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

	private static bool MatchesGenericParameter(Type type, Type genericType, Dictionary<Type, Type?>? map = null)
	{
		// Test interfaces
		foreach (var genericInterface in genericType.GetInterfaces())
		{
			var @interface = genericInterface.IsGenericType
				? genericInterface.GetGenericTypeDefinition()
				: genericInterface;

			Type? matchedInterface = null;

			foreach (var typeInterface in type.GetInterfaces())
			{
				if (typeInterface.IsGenericType && typeInterface.GetGenericTypeDefinition() == @interface)
				{
					if (!UpdateMap(map, genericInterface, typeInterface))
					{
						return false;
					}

					matchedInterface = typeInterface;
					break;
				}

				if (!typeInterface.IsGenericType && typeInterface == @interface)
				{
					matchedInterface = typeInterface;
					break;
				}
			}

			if (matchedInterface == null)
			{
				return false;
			}
		}

		// Test base types
		if (genericType.BaseType is not null)
		{
			var @base = genericType.BaseType.IsGenericType
				? genericType.BaseType.GetGenericTypeDefinition()
				: genericType.BaseType;

			Type? matchedBase = null;

			for (var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
			{
				if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == @base)
				{
					if (!UpdateMap(map, genericType.BaseType, baseType))
					{
						return false;
					}

					matchedBase = baseType;
					break;
				}

				if (!baseType.IsGenericType && baseType == @base)
				{
					matchedBase = baseType;
					break;
				}
			}

			if (matchedBase == null)
			{
				return false;
			}
		}

		return true;
	}

	private static bool UpdateMap(Dictionary<Type, Type?>? map, Type genericInterface, Type typeInterface)
	{
		if (map is null)
		{
			return true;
		}

		var genericArguments = genericInterface.GetGenericArguments();
		var typeArguments = typeInterface.GetGenericArguments();

		for (var i = 0; i < genericArguments.Length; i++)
		{
			if (!map.TryGetValue(genericArguments[i], out var value)) continue;

			if (value is null)
			{
				map[genericArguments[i]] = typeArguments[i];
			}
			else if (value != typeArguments[i])
			{
				return false;
			}
		}

		return true;
	}
}