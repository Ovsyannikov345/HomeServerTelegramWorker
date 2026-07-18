using HomeLabCore.Application.Telegram.CallbackQueryHandlers;
using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Abstractions;
using HomeLabCore.Application.Telegram.CommandHandlers;
using HomeLabCore.Application.Telegram.CommandHandlers.Abstractions;
using HomeLabCore.Worker.TelegramPolling.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HomeLabCore.Worker.TelegramPolling;

internal sealed class TelegramPollingWorker(
    IServiceScopeFactory scopeFactory,
    ITelegramBotClient telegramBotClient,
    ILogger<TelegramPollingWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
            DropPendingUpdates = false
        };

        try
        {
            await telegramBotClient.ReceiveAsync(
                updateHandler: HandleUpdate,
                errorHandler: HandlePollingError,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken
            );
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Stopping polling because application is shutting down.");
        }
    }

    private async Task HandleUpdate(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        try
        {
            if (update.TryExtractCommand(out var command))
            {
                await HandleCommand(command, ct);
            }
            else if (update.TryExtractCallbackQuery(out var callbackQuery))
            {
                await HandleCallbackQuery(callbackQuery, ct);
            }
            else
            {
                logger.LogError("Update {UpdateId} can't be handled because of unknown update type.", update.Id);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling update {UpdateId}", update.Id);
        }
    }

    async Task HandleCommand(Message message, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var commandHandlers = scope.ServiceProvider.GetServices<ICommandHandler>();

        if (commandHandlers.FirstOrDefault(h => h.CanHandle(message)) is { } capableHandler)
        {
            await capableHandler.Handle(message, ct);
        }
        else
        {
            var fallbackHandler = scope.ServiceProvider.GetRequiredService<IFallbackCommandHandler>();

            await fallbackHandler.Handle(message, ct);
        }
    }

    async Task HandleCallbackQuery(CallbackQuery callbackQuery, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var callbackQueryHandlers = scope.ServiceProvider.GetServices<ICallbackQueryHandler>();

        if (callbackQueryHandlers.FirstOrDefault(h => h.CanHandle(callbackQuery)) is { } capableHandler)
        {
            await capableHandler.Handle(callbackQuery, ct);
        }
        else
        {
            var fallbackHandler = scope.ServiceProvider.GetRequiredService<IFallbackCallbackQueryHandler>();

            await fallbackHandler.Handle(callbackQuery, ct);
        }
    }

    private Task HandlePollingError(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Telegram API Polling Error");

        return Task.CompletedTask;
    }
}
