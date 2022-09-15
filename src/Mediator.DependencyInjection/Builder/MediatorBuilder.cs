using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

public partial class MediatorBuilder : IMediatorBuilder
{
    private readonly IServiceCollection _services;
    private readonly MediatorNamespace? _ns;

    public MediatorBuilder(IServiceCollection services, MediatorNamespace? ns = null)
    {
        _services = services;
        _ns = ns;
    }

    public IMediatorBuilder WithNamespace(string ns)
    {
        return new MediatorBuilder(_services, new MediatorNamespace(ns));
    }

    public IMediatorBuilder AddNamespace(MediatorNamespace ns)
    {
        return new MediatorBuilder(_services, ns);
    }
}
