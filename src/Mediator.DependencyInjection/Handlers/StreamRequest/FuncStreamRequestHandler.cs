using System;
using System.Collections.Generic;
using System.Threading;
using MediatR;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

internal class FuncStreamRequestHandler<TRequest, TResponse> : IStreamRequestHandler<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<IServiceProvider, TRequest, IAsyncEnumerable<TResponse>> _func;

    public FuncStreamRequestHandler(Func<IServiceProvider, TRequest, IAsyncEnumerable<TResponse>> func, IServiceProvider serviceProvider)
    {
        _func = func;
        _serviceProvider = serviceProvider;
    }

    public IAsyncEnumerable<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        return _func(_serviceProvider, request);
    }
}
