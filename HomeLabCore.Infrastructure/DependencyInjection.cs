using HomeLabCore.Application.Interfaces.Clients;
using HomeLabCore.Application.Interfaces.Database;
using HomeLabCore.Infrastructure.Common.Constants;
using HomeLabCore.Infrastructure.Database;
using HomeLabCore.Infrastructure.Database.Interceptors;
using HomeLabCore.Infrastructure.Seerr;
using HomeLabCore.Infrastructure.Seerr.Configuration;
using HomeLabCore.Infrastructure.Seerr.Handlers;
using HomeLabCore.Infrastructure.Telegram;
using HomeLabCore.Infrastructure.Telegram.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace HomeLabCore.Infrastructure;

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

        // Database
        services.AddSingleton<AuditableEntityInterceptor>();

        services.AddDbContext<IApplicationDbContext, ApplicationDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options
                .UseNpgsql(configuration.GetConnectionString(ConnectionStringNames.Database))
                .AddInterceptors(interceptor);
        });

        return services;
    }
}
