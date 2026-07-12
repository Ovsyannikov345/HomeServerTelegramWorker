using HomeLabNotifier.Application;
using HomeLabNotifier.Infrastructure;

namespace HomeLabNotifier.Api;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureApplication(this IHostApplicationBuilder applicationBuilder)
    {
        var configuration = applicationBuilder.Configuration;

        return applicationBuilder.Services
            .AddInfrastructureServices(configuration)
            .AddApplicationServices()
            .AddApiServices();
    }

    private static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        return services.AddOpenApi();
    }
}
