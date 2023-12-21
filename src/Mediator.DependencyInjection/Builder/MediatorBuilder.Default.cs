using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Zapto.Mediator;

public partial class MediatorBuilder : IMediatorBuilder
{
	public IMediatorBuilder AddDefaultRequestHandler(Type handlerType)
	{
		_services.AddTransient(typeof(IDefaultRequestHandler), handlerType);
		return this;
	}

	public IMediatorBuilder AddDefaultRequestHandler<THandler>() where THandler : class, IDefaultRequestHandler
	{
		_services.AddTransient<IDefaultRequestHandler, THandler>();
		return this;
	}

	public IMediatorBuilder AddDefaultNotificationHandler(Type handlerType)
	{
		_services.AddTransient(typeof(IDefaultNotificationHandler), handlerType);
		return this;
	}

	public IMediatorBuilder AddDefaultNotificationHandler<THandler>() where THandler : class, IDefaultNotificationHandler
	{
		_services.AddTransient<IDefaultNotificationHandler, THandler>();
		return this;
	}

	public IMediatorBuilder AddDefaultStreamRequestHandler(Type handlerType)
	{
		_services.AddTransient(typeof(IDefaultStreamRequestHandler), handlerType);
		return this;
	}

	public IMediatorBuilder AddDefaultStreamRequestHandler<THandler>() where THandler : class, IDefaultStreamRequestHandler
	{
		_services.AddTransient<IDefaultStreamRequestHandler, THandler>();
		return this;
	}
}