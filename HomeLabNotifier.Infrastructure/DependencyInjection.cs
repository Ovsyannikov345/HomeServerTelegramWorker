using HomeLabNotifier.Application.Interfaces.Clients;
using HomeLabNotifier.Infrastructure.Seerr;
using HomeLabNotifier.Infrastructure.Seerr.Configuration;
using HomeLabNotifier.Infrastructure.Seerr.Handlers;
using HomeLabNotifier.Infrastructure.Telegram;
using HomeLabNotifier.Infrastructure.Telegram.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace HomeLabNotifier.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services
            .AddOptions<TelegramSettings>()
            .Bind(configuration.GetSection(TelegramSettings.SectionName))
            .ValidateOnStart()
            .ValidateDataAnnotations();

        services.AddOptions<SeerrSettings>()
            .Bind(configuration.GetSection(SeerrSettings.SectionName))
            .ValidateOnStart()
            .ValidateDataAnnotations();

        // HTTP Clients
        services.AddTransient<SeerrAuthorizationHandler>();

        services.AddHttpClient<IMediaManagerClient, SeerrClient>((serviceProvider, client) =>
        {
            var seerSettings = serviceProvider.GetRequiredService<IOptionsMonitor<SeerrSettings>>();

            client.BaseAddress = new Uri(seerSettings.CurrentValue.BaseUrl, UriKind.Absolute);
        }).AddHttpMessageHandler<SeerrAuthorizationHandler>();

        // Telegram
        services.AddSingleton<ITelegramBotClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<TelegramSettings>>().Value;

            return new TelegramBotClient(settings.BotToken);
        });

        // Background Services
        services.AddHostedService<TelegramPollingWorker>();

        return services;
    }
}
