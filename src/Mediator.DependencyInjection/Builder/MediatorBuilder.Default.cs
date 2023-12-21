using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

public partial class MediatorBuilder : IMediatorBuilder
{
	public IMediatorBuilder AddDefaultRequestHandler(Type handlerType, RegistrationScope scope = RegistrationScope.Transient)
	{
		_services.Add(new ServiceDescriptor(typeof(IDefaultRequestHandler), handlerType, GetLifetime(scope)));
		return this;
	}

	public IMediatorBuilder AddDefaultRequestHandler<THandler>(RegistrationScope scope = RegistrationScope.Transient) where THandler : class, IDefaultRequestHandler
	{
		_services.Add(new ServiceDescriptor(typeof(IDefaultRequestHandler), typeof(THandler), GetLifetime(scope)));
		return this;
	}

	public IMediatorBuilder AddDefaultNotificationHandler(Type handlerType, RegistrationScope scope = RegistrationScope.Transient)
	{
		_services.Add(new ServiceDescriptor(typeof(IDefaultNotificationHandler), handlerType, GetLifetime(scope)));
		return this;
	}

	public IMediatorBuilder AddDefaultNotificationHandler<THandler>(RegistrationScope scope = RegistrationScope.Transient) where THandler : class, IDefaultNotificationHandler
	{
		_services.Add(new ServiceDescriptor(typeof(IDefaultNotificationHandler), typeof(THandler), GetLifetime(scope)));
		return this;
	}

	public IMediatorBuilder AddDefaultStreamRequestHandler(Type handlerType, RegistrationScope scope = RegistrationScope.Transient)
	{
		_services.Add(new ServiceDescriptor(typeof(IDefaultStreamRequestHandler), handlerType, GetLifetime(scope)));
		return this;
	}

	public IMediatorBuilder AddDefaultStreamRequestHandler<THandler>(RegistrationScope scope = RegistrationScope.Transient) where THandler : class, IDefaultStreamRequestHandler
	{
		_services.Add(new ServiceDescriptor(typeof(IDefaultStreamRequestHandler), typeof(THandler), GetLifetime(scope)));
		return this;
	}
}