using HomeLabCore.Worker.TelegramPolling;
using Microsoft.Extensions.DependencyInjection;

namespace HomeLabCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkerServices(this IServiceCollection services)
    {
        // Background Services
        services.AddHostedService<TelegramPollingWorker>();

        return services;
    }
}
