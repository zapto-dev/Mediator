using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public static partial class ServiceExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddTransient<IMediator, ServiceProviderMediator>();
        services.AddTransient<ISender, ServiceProviderMediator>();
        services.AddTransient<IPublisher, ServiceProviderMediator>();

        services.AddSingleton<GenericRequestCache>();
        services.AddTransient(typeof(IRequestHandler<,>), typeof(GenericRequestHandler<,>));

        services.AddSingleton<GenericNotificationCache>();
        services.AddTransient(typeof(GenericNotificationHandler<>));

        return services;
    }
}
