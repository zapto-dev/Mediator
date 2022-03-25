using System;
using Mediator.DependencyInjection.Tests.Delegates;
using MediatR;

namespace Mediator.DependencyInjection.Tests.Generics;

public record struct ReturnRequest(string Value) : IRequest<string>;
