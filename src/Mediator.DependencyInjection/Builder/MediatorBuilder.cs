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

    public IServiceCollection Services => _services;

    public IMediatorBuilder WithNamespace(string ns)
    {
        return new MediatorBuilder(_services, new MediatorNamespace(ns));
    }

    public IMediatorBuilder AddNamespace(MediatorNamespace ns)
    {
        return new MediatorBuilder(_services, ns);
    }

    private ServiceLifetime GetLifetime(RegistrationScope scope)
    {
        return scope switch
        {
            RegistrationScope.Transient => ServiceLifetime.Transient,
            RegistrationScope.Singleton => ServiceLifetime.Singleton,
            RegistrationScope.Scoped => ServiceLifetime.Scoped,
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
        };
    }
}
