﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public partial class MediatorBuilder
{
    private static readonly ConstructorInfo NewValueTaskConstructor = typeof(ValueTask).GetConstructor(new[] {typeof(Task)})!;
    private static readonly MethodInfo CastValueTaskMethod = typeof(MediatorBuilder).GetMethod(nameof(CastValueTask), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo CastTaskMethod = typeof(MediatorBuilder).GetMethod(nameof(CastTask), BindingFlags.NonPublic | BindingFlags.Static)!;

    private void RegisterHandler(
        string registerMethodName,
        Type[] parameterTypeTargets,
        string noResultMessage,
        string multipleResultMessage,
        Delegate handler)
    {
        var method = handler.GetMethodInfo();
        var parameterTargets = method
            .GetParameters()
            .Select(i =>
            {
                var type = i.ParameterType.GetInterfaces().FirstOrDefault(t1 =>
                    parameterTypeTargets.Any(t2 =>
                        t2.IsGenericType == t1.IsGenericType && (
                            t1.IsGenericType
                                ? t1.GetGenericTypeDefinition() == t2
                                : t1 == t2
                        )));

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
        var cancellationToken = Expression.Parameter(typeof(CancellationToken));
        var getService = typeof(ServiceProviderServiceExtensions).GetMethods()
            .First(i => i.Name == "GetRequiredService" && i.IsGenericMethod);

        var parameters = method.GetParameters()
            .Select<ParameterInfo, Expression>(i =>
            {
                if (i.ParameterType == type) return notification;
                if (i.ParameterType == typeof(IServiceProvider)) return serviceProvider;
                if (i.ParameterType == typeof(CancellationToken)) return cancellationToken;
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
                else if (CanConvertFrom(resultType, fromType, out var converter))
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
                else if (CanConvertFrom(resultType, fromType, out var converter))
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
            else if (CanConvertFrom(resultType, call.Type, out _))
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

        var lambda = Expression.Lambda(result, serviceProvider, notification, cancellationToken).Compile();
        var registerMethod = typeof(MediatorBuilder).GetMethods()
            .First(i =>
                i.Name == registerMethodName && i.IsGenericMethod &&
                i.GetGenericArguments().Length == (interfaceType.IsGenericType ? 1 + interfaceType.GetGenericArguments().Length : 1) &&
                i.GetParameters().FirstOrDefault() is {ParameterType: {IsGenericType: true} p} &&
                p.GetGenericTypeDefinition() == typeof(Func<,,,>));

        registerMethod
            .MakeGenericMethod(resultType is null
                ? new[] { type }
                : new[] { type, resultType })
            .Invoke(this, new object?[] {lambda});
    }

    private static async ValueTask<TTo> CastValueTask<TFrom, TTo>(ValueTask<TFrom> result, Func<TFrom, TTo> converter)
        => converter(await result);

    private static async ValueTask<TTo> CastTask<TFrom, TTo>(Task<TFrom> result, Func<TFrom, TTo> converter)
        => converter(await result);

    private static bool CanConvertFrom(Type to, Type from, out object converter)
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
