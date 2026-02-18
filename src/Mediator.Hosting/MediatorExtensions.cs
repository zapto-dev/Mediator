using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zapto.Mediator.Options;
using Zapto.Mediator.Services;

namespace Zapto.Mediator;

public static class MediatorExtensions
{
    public static IMediatorBuilder AddHostingBackgroundScheduler(this IMediatorBuilder builder, Action<MediatorBackgroundOptions>? configure = null)
    {
        if (builder is not MediatorBuilder mediatorBuilder)
        {
            throw new InvalidOperationException("Mediator.Hosting requires Mediator.DependencyInjection to be registered.");
        }

        mediatorBuilder.Services.AddSingleton<BackgroundQueueService>();
        mediatorBuilder.Services.AddHostedService<BackgroundQueueHostedService>();
        mediatorBuilder.Services.Replace(ServiceDescriptor.Singleton<IBackgroundPublisher, BackgroundPublisher>());

        if (configure is not null)
        {
            mediatorBuilder.Services.Configure(configure);
        }

        return builder;
    }
}
