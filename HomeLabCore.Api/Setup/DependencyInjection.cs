using HomeLabCore.Application;
using HomeLabCore.Infrastructure;
using Serilog;

namespace HomeLabCore.Api.Setup;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureApplication(this WebApplicationBuilder applicationBuilder)
    {
        Log.Information("Configuring the application services...");

        AddLogging(applicationBuilder);

        var configuration = applicationBuilder.Configuration;

        return applicationBuilder.Services
            .AddInfrastructureServices(configuration)
            .AddApplicationServices(configuration)
            .AddWorkerServices()
            .AddApiServices();
    }

    private static void AddLogging(this WebApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());
    }

    private static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        Log.Information("Configuring HomeLabCore.API services...");

        return services.AddOpenApi();
    }
}
