using HomeLabCore.Worker.TelegramPolling;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace HomeLabCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkerServices(this IServiceCollection services)
    {
        Log.Information("Configuring HomeLabCore.Worker services...");

        // Background Services
        services.AddHostedService<TelegramPollingWorker>();

        return services;
    }
}
