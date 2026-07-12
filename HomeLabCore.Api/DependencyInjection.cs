using HomeLabCore.Application;
using HomeLabCore.Infrastructure;

namespace HomeLabCore.Api;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureApplication(this IHostApplicationBuilder applicationBuilder)
    {
        var configuration = applicationBuilder.Configuration;

        return applicationBuilder.Services
            .AddInfrastructureServices(configuration)
            .AddApplicationServices(configuration)
            .AddApiServices();
    }

    private static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        return services.AddOpenApi();
    }
}
