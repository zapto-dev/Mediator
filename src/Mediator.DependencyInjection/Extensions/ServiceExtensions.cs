using Microsoft.Extensions.DependencyInjection;

namespace Zapto.Mediator;

public static partial class ServiceExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddScoped<IMediator, ServiceProviderMediator>();
        services.AddScoped<ISender>(i => i.GetRequiredService<IMediator>());
        services.AddScoped<IPublisher>(i => i.GetRequiredService<IMediator>());
        services.AddScoped(typeof(IRequestHandler<,>), typeof(GenericRequestHandler<,>));
        return services;
    }
}
