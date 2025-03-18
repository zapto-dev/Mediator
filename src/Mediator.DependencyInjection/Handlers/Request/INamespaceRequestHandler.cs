using System;
using MediatR;

namespace Zapto.Mediator;

public interface INamespaceRequestHandler<in TRequest> : INamespaceHandler
    where TRequest : IRequest
{
    IRequestHandler<TRequest> GetHandler(IServiceProvider provider);
}

public interface INamespaceRequestHandler<in TRequest, TResponse> : INamespaceHandler
    where TRequest : IRequest<TResponse>
{
    IRequestHandler<TRequest, TResponse> GetHandler(IServiceProvider provider);
}
