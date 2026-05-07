using Microsoft.Extensions.DependencyInjection;

namespace AntDesign.X;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAntDesignX(
        this IServiceCollection services,
        Action<XRequestClientOptions>? configureRequest = null)
    {
        var requestOptions = new XRequestClientOptions();
        configureRequest?.Invoke(requestOptions);

        services.AddSingleton(requestOptions);
        services.AddHttpClient(requestOptions.HttpClientName, client =>
        {
            if (requestOptions.BaseAddress is not null)
            {
                client.BaseAddress = requestOptions.BaseAddress;
            }

            if (requestOptions.Timeout > TimeSpan.Zero)
            {
                client.Timeout = requestOptions.Timeout;
            }

            foreach (var header in requestOptions.DefaultHeaders)
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        });
        services.AddScoped<IXNotificationService, XNotificationService>();
        services.AddScoped<IXRequestClient, XRequestClient>();
        services.AddScoped<XChatStore>();
        services.AddScoped<XAgentStore>();
        return services;
    }
}
