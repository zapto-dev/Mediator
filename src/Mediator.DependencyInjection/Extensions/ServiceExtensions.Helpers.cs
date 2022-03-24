using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public static partial class ServiceExtensions
{
    private static void RegisterHandler(
        string registerMethodName,
        Type parameterTypeTarget,
        string noResultMessage,
        string multipleResultMessage,
        IServiceCollection services,
        Delegate handler,
        MediatorNamespace? ns)
    {
        var method = handler.GetMethodInfo();
        var parameterTargets = method
            .GetParameters()
            .Select(i =>
            {
                var type = i.ParameterType.GetInterfaces().FirstOrDefault(t =>
                    parameterTypeTarget.IsGenericType == t.IsGenericType && (
                        t.IsGenericType
                            ? t.GetGenericTypeDefinition() == parameterTypeTarget
                            : t == parameterTypeTarget
                    ));

                return (Type: type!, Parameter: i);
            })
            .Where(i => i.Type is not null)
            .ToList();

        if (parameterTargets.Count == 0)
        {
            throw new InvalidOperationException(noResultMessage);
        }

        if (parameterTargets.Count > 1)
        {
            throw new InvalidOperationException(multipleResultMessage);
        }

        var (interfaceType, parameter) = parameterTargets[0];
        var type = parameter.ParameterType;
        var serviceProvider = Expression.Parameter(typeof(IServiceProvider));
        var notification = Expression.Parameter(type);
        var getService = typeof(ServiceProviderServiceExtensions).GetMethods()
            .First(i => i.Name == "GetRequiredService" && i.IsGenericMethod);

        var parameters = method.GetParameters()
            .Select<ParameterInfo, Expression>(i =>
            {
                if (i.ParameterType == type) return notification;
                if (i.ParameterType == typeof(IServiceProvider)) return serviceProvider;
                return Expression.Call(getService.MakeGenericMethod(i.ParameterType), serviceProvider);
            })
            .ToArray();

        var call = Expression.Call(Expression.Constant(handler.Target), method, parameters);
        Expression result;

        Type? resultType = null;

        if (!interfaceType.IsGenericType)
        {
            if (call.Type == typeof(ValueTask))
            {
                result = call;
            }
            else if (call.Type.IsGenericType && call.Type.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                result = Expression.Convert(call, typeof(ValueTask));
            }
            else if (call.Type == typeof(Task) ||
                     call.Type.IsGenericType && call.Type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                result = Expression.New(typeof(ValueTask).GetConstructor(new[] {typeof(Task)}), call);
            }
            else
            {
                result = Expression.Block(call, Expression.Default(typeof(ValueTask)));
            }
        }
        else
        {
            resultType = interfaceType.GetGenericArguments()[0];

            if (call.Type.IsGenericType && call.Type.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                var valueType = call.Type.GetGenericArguments()[0];

                if (valueType == resultType)
                {
                    result = call;
                }
                else if (resultType.IsAssignableFrom(valueType))
                {
                    result = Expression.Call(
                        typeof(ServiceExtensions).GetMethod(nameof(CastValueTask))
                            .MakeGenericMethod(resultType, valueType),
                        call
                    );
                }
                else
                {
                    throw new InvalidOperationException($"Cannot cast result {valueType.Name} to {resultType.Name}");
                }
            }
            else if (call.Type.IsGenericType && call.Type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var valueType = call.Type.GetGenericArguments()[0];

                if (valueType == resultType)
                {
                    result = Expression.New(
                        typeof(ValueTask<>)
                            .MakeGenericType(resultType)
                            .GetConstructor(new[] {typeof(Task)}),
                        call);
                }
                else if (resultType.IsAssignableFrom(valueType))
                {
                    result = Expression.Call(
                        typeof(ServiceExtensions).GetMethod(nameof(CastTask))
                            .MakeGenericMethod(resultType, valueType),
                        call
                    );
                }
                else
                {
                    throw new InvalidOperationException($"Cannot cast result {valueType.Name} to {resultType.Name}");
                }
            }
            else if (resultType == call.Type || resultType.IsAssignableFrom(call.Type))
            {
                result = Expression.New(
                    typeof(ValueTask<>)
                        .MakeGenericType(resultType)
                        .GetConstructor(new[] { resultType }),
                    call);
            }
            else
            {
                throw new InvalidOperationException($"Cannot cast {call.Type.Name} to {resultType.Name}");
            }
        }

        var lambda = Expression.Lambda(result, serviceProvider, notification).Compile();
        var registerMethod = typeof(ServiceExtensions).GetMethods()
            .First(i =>
                i.Name == registerMethodName && i.IsGenericMethod &&
                i.GetParameters().ElementAtOrDefault(1) is {ParameterType: {IsGenericType: true} p} &&
                p.GetGenericTypeDefinition() == typeof(Func<,,>));

        registerMethod
            .MakeGenericMethod(resultType is null
                ? new[] { type }
                : new[] { type, resultType })
            .Invoke(null, new object?[] {services, lambda, ns});
    }

    public static async ValueTask<T1> CastValueTask<T1, T2>(ValueTask<T2> result) where T2 : T1 => await result;

    public static async ValueTask<T1> CastTask<T1, T2>(Task<T2> result) where T2 : T1 => await result;
}
