using Microsoft.Extensions.DependencyInjection;

namespace AntDesign.X;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAntDesignX(this IServiceCollection services)
    {
        services.AddScoped<IXNotificationService, XNotificationService>();
        return services;
    }
}
