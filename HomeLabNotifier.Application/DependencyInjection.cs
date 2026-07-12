using HomeLabNotifier.Application.Telegram;
using HomeLabNotifier.Application.Telegram.CallbackQueryHandlers;
using HomeLabNotifier.Application.Telegram.CommandHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace HomeLabNotifier.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Handlers
        services
            .AddScoped<ICommandHandler, HelpCommandHandler>()
            .AddScoped<ICommandHandler, SearchCommandHandler>();

        services.AddScoped<ICallbackQueryHandler, RequestMediaQueryHandler>();

        services.AddScoped<IFallbackHandler, FallbackHandler>();

        return services;
    }
}
