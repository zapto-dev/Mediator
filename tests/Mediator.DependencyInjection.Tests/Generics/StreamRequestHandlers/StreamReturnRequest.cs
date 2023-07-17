using System;
using Mediator.DependencyInjection.Tests.Delegates;
using MediatR;

namespace Mediator.DependencyInjection.Tests.Generics;

public record struct ReturnStreamRequest(string Value) : IStreamRequest<string>;
