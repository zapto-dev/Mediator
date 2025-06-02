using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

internal class FuncRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly Func<IServiceProvider, TRequest, CancellationToken, ValueTask<TResponse>> _invoke;

    public FuncRequestHandler(Func<IServiceProvider, TRequest, CancellationToken, ValueTask<TResponse>> invoke)
    {
        _invoke = invoke;
    }

    public ValueTask<TResponse> Handle(IServiceProvider provider, TRequest request, CancellationToken cancellationToken)
    {
        return _invoke(provider, request, cancellationToken);
    }
}

internal class FuncRequestHandler<TRequest> : IRequestHandler<TRequest>
    where TRequest : IRequest
{
    private readonly Func<IServiceProvider, TRequest, CancellationToken, ValueTask> _invoke;

    public FuncRequestHandler(Func<IServiceProvider, TRequest, CancellationToken, ValueTask> invoke)
    {
        _invoke = invoke;
    }

    public ValueTask Handle(IServiceProvider provider, TRequest request, CancellationToken cancellationToken)
    {
        return _invoke(provider, request, cancellationToken);
    }
}
