using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public static partial class ServiceExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddTransient<IMediator, ServiceProviderMediator>();
        services.AddTransient<ISender, ServiceProviderMediator>();
        services.AddTransient<IPublisher, ServiceProviderMediator>();
        services.AddTransient(typeof(IRequestHandler<,>), typeof(GenericRequestHandler<,>));
        return services;
    }
}
