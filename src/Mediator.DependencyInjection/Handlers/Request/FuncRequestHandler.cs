using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

internal class FuncRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<IServiceProvider, TRequest, ValueTask<TResponse>> _invoke;

    public FuncRequestHandler(Func<IServiceProvider, TRequest, ValueTask<TResponse>> invoke, IServiceProvider serviceProvider)
    {
        _invoke = invoke;
        _serviceProvider = serviceProvider;
    }

    public ValueTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        return _invoke(_serviceProvider, request);
    }
}
