﻿using System;
using MediatR;

namespace Zapto.Mediator;

public interface INamespaceStreamRequestHandler<in TRequest, out TResponse> : INamespaceHandler
    where TRequest : IStreamRequest<TResponse>
{
    IStreamRequestHandler<TRequest, TResponse> GetHandler(IServiceProvider provider);
}
