using System;
using System.Collections.Generic;
using System.Threading;
using MediatR;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

internal class FuncStreamRequestHandler<TRequest, TResponse> : IStreamRequestHandler<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    private readonly Func<IServiceProvider, TRequest, IAsyncEnumerable<TResponse>> _func;

    public FuncStreamRequestHandler(Func<IServiceProvider, TRequest, IAsyncEnumerable<TResponse>> func)
    {
        _func = func;
    }

    public IAsyncEnumerable<TResponse> Handle(IServiceProvider provider, TRequest request, CancellationToken cancellationToken)
    {
        return _func(provider, request);
    }
}
