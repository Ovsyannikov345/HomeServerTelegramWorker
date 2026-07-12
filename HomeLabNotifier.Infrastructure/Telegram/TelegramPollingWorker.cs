using HomeLabNotifier.Application.Telegram;
using HomeLabNotifier.Application.Telegram.CallbackQueryHandlers;
using HomeLabNotifier.Application.Telegram.CommandHandlers;
using HomeLabNotifier.Infrastructure.Telegram.Configuration;
using HomeLabNotifier.Infrastructure.Telegram.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HomeLabNotifier.Infrastructure.Telegram;

internal sealed class TelegramPollingWorker(
    IServiceScopeFactory scopeFactory,
    ITelegramBotClient telegramBotClient,
    IOptionsMonitor<TelegramSettings> options,
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
        long? chatId = update.Type switch
        {
            UpdateType.Message => update.Message?.Chat.Id,
            UpdateType.CallbackQuery => update.CallbackQuery?.Message?.Chat.Id,
            _ => null
        };

        // TODO add auth message handler
        if (chatId is null)
        {
            logger.LogWarning("Unauthorized access attempt from unknown chat.");

            return;
        }

        if (!options.CurrentValue.ChatIdWhitelist.Any(id => id == chatId.Value))
        {
            logger.LogWarning("Unauthorized access attempt from Chat ID: {ChatId}", chatId);

            return;
        }

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
            var fallbackHandler = scope.ServiceProvider.GetRequiredService<IFallbackHandler>();

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
            // TODO figure out the handling approach.
        }
    }

    private Task HandlePollingError(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Telegram API Polling Error");

        return Task.CompletedTask;
    }
}
