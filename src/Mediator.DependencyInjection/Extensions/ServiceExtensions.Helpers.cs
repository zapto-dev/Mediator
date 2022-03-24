using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public static partial class ServiceExtensions
{
    private static readonly ConstructorInfo NewValueTaskConstructor = typeof(ValueTask).GetConstructor(new[] {typeof(Task)})!;
    private static readonly MethodInfo CastValueTaskMethod = typeof(ServiceExtensions).GetMethod(nameof(CastValueTask), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo CastTaskMethod = typeof(ServiceExtensions).GetMethod(nameof(CastTask), BindingFlags.NonPublic | BindingFlags.Static)!;

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
                result = Expression.New(NewValueTaskConstructor, call);
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
                var fromType = call.Type.GetGenericArguments()[0];

                if (fromType == resultType)
                {
                    result = call;
                }
                else if (resultType.CanConvertFrom(fromType, out var converter))
                {
                    result = Expression.Call(
                        CastValueTaskMethod.MakeGenericMethod(fromType, resultType),
                        call,
                        Expression.Constant(converter)
                    );
                }
                else
                {
                    throw new InvalidOperationException($"Cannot cast result {fromType.Name} to {resultType.Name}");
                }
            }
            else if (call.Type.IsGenericType && call.Type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var fromType = call.Type.GetGenericArguments()[0];

                if (fromType == resultType)
                {
                    result = Expression.New(
                        typeof(ValueTask<>)
                            .MakeGenericType(resultType)
                            .GetConstructor(new[] { typeof(Task<>).MakeGenericType(resultType) })!,
                        call);
                }
                else if (resultType.CanConvertFrom(fromType, out var converter))
                {
                    result = Expression.Call(
                        CastTaskMethod.MakeGenericMethod(fromType, resultType),
                        call,
                        Expression.Constant(converter)
                    );
                }
                else
                {
                    throw new InvalidOperationException($"Cannot cast result {fromType.Name} to {resultType.Name}");
                }
            }
            else if (resultType == call.Type)
            {
                result = Expression.New(
                    typeof(ValueTask<>)
                        .MakeGenericType(resultType)
                        .GetConstructor(new[] { resultType })!,
                    call);
            }
            else if (resultType.CanConvertFrom(call.Type, out _))
            {
                result = Expression.New(
                    typeof(ValueTask<>)
                        .MakeGenericType(resultType)
                        .GetConstructor(new[] { resultType })!,
                    Expression.Convert(call, resultType));
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

    private static async ValueTask<TTo> CastValueTask<TFrom, TTo>(ValueTask<TFrom> result, Func<TFrom, TTo> converter)
        => converter(await result);

    private static async ValueTask<TTo> CastTask<TFrom, TTo>(Task<TFrom> result, Func<TFrom, TTo> converter)
        => converter(await result);

    private static bool CanConvertFrom(this Type to, Type from, out object converter)
    {
        UnaryExpression BodyFunction(Expression body) => Expression.Convert(body, to);

        try
        {
            var x = Expression.Parameter(from, "x");
            converter = Expression.Lambda(BodyFunction(x), x).Compile();
            return true;
        }
        catch (InvalidOperationException)
        {
            converter = null!;
            return false;
        }
    }
}
