using HomeLabCore.Application.Telegram.CallbackQueryHandlers;
using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Abstractions;
using HomeLabCore.Application.Telegram.CommandHandlers;
using HomeLabCore.Application.Telegram.CommandHandlers.Abstractions;
using HomeLabCore.Shared.Constants;
using HomeLabCore.Shared.Contexts;
using HomeLabCore.Worker.Logging;
using HomeLabCore.Worker.TelegramPolling.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WorkerLogPropertyNames = HomeLabCore.Worker.Constants.LogPropertyNames;

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
            logger.StartingTelegramUpdatePolling();

            await telegramBotClient.ReceiveAsync(
                updateHandler: HandleUpdate,
                errorHandler: HandlePollingError,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken
            );
        }
        catch (OperationCanceledException)
        {
            logger.TelegramUpdatePollingStopped();
        }
        catch (Exception ex)
        {
            logger.TelegramUpdatePollingWorkerCrashed(ex);
        }
    }

    private async Task HandleUpdate(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        using var correlationContext = LogContext.PushProperty(LogPropertyNames.CorrelationId, CorrelationContext.CorrelationId);
        using var updateIdContext = LogContext.PushProperty(WorkerLogPropertyNames.TelegramUpdateId, update.Id);

        try
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.TelegramUpdateReceived(update.Id, JsonSerializer.Serialize(update));
            }

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
                logger.TelegramUpdateHandlingFailed(update.Id, "Failed to determine the update type");
            }
        }
        catch (Exception ex)
        {
            logger.TelegramUpdateHandlingFailed(update.Id, ex);
        }
    }

    async Task HandleCommand(Message message, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        using var messageIdContext = LogContext.PushProperty(WorkerLogPropertyNames.TelegramMessageId, message.Id);

        var commandHandlers = scope.ServiceProvider.GetServices<ICommandHandler>();

        if (commandHandlers.FirstOrDefault(h => h.CanHandle(message)) is { } capableHandler)
        {
            await capableHandler.Handle(message, ct);
        }
        else
        {
            logger.FailedToDetermineCommandHandler(message.Text);

            var fallbackHandler = scope.ServiceProvider.GetRequiredService<IFallbackCommandHandler>();

            await fallbackHandler.Handle(message, ct);
        }
    }

    async Task HandleCallbackQuery(CallbackQuery callbackQuery, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        using var queryIdContext = LogContext.PushProperty(WorkerLogPropertyNames.TelegramCallbackQueryId, callbackQuery.Id);

        var callbackQueryHandlers = scope.ServiceProvider.GetServices<ICallbackQueryHandler>();

        if (callbackQueryHandlers.FirstOrDefault(h => h.CanHandle(callbackQuery)) is { } capableHandler)
        {
            await capableHandler.Handle(callbackQuery, ct);
        }
        else
        {
            logger.FailedToDetermineCallbackQueryHandler(callbackQuery.Data);

            var fallbackHandler = scope.ServiceProvider.GetRequiredService<IFallbackCallbackQueryHandler>();

            await fallbackHandler.Handle(callbackQuery, ct);
        }
    }

    private Task HandlePollingError(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        using var correlationContext = LogContext.PushProperty(LogPropertyNames.CorrelationId, CorrelationContext.CorrelationId);

        logger.TelegramUpdatePollingFailed(exception);

        return Task.CompletedTask;
    }
}
