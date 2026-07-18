using HomeLabCore.Application.Telegram.CallbackQueryHandlers;
using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Abstractions;
using HomeLabCore.Application.Telegram.CommandHandlers;
using HomeLabCore.Application.Telegram.CommandHandlers.Abstractions;
using HomeLabCore.Application.Telegram.Configuration;
using HomeLabCore.Application.Telegram.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeLabCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services
            .AddOptions<TelegramSettings>()
            .Bind(configuration.GetSection(TelegramSettings.SectionName))
            .ValidateOnStart()
            .ValidateDataAnnotations();

        // Services
        services.AddScoped<IMessageRenderer, MessageRenderer>();

        // Handlers
        services.RegisterTelegramHandlers();

        return services;
    }

    private static IServiceCollection RegisterTelegramHandlers(this IServiceCollection services)
    {
        // Command Handlers
        services
            .AddScoped<ICommandHandler, StartCommandHandler>()
            .AddScoped<ICommandHandler, HelpCommandHandler>()
            .AddScoped<ICommandHandler, SearchCommandHandler>();

        // Query Handlers
        services
            .AddScoped<ICallbackQueryHandler, EmptyQueryHandler>()
            .AddScoped<ICallbackQueryHandler, RequestMediaQueryHandler>()
            .AddScoped<ICallbackQueryHandler, ChangeSearchPageQueryHandler>();

        // Other handlers
        services.AddScoped<IFallbackCommandHandler, FallbackCommandHandler>();
        services.AddScoped<IFallbackCallbackQueryHandler, FallbackCallbackQueryHandler>();

        return services;
    }
}
