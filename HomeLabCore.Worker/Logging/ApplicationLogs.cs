using Microsoft.Extensions.Logging;

namespace HomeLabCore.Worker.Logging;

internal static partial class ApplicationLogs
{
    #region Telegram polling and update routing

    [LoggerMessage(
        EventId = 1_0001_0001,
        Level = LogLevel.Information,
        Message = "Telegram polling is starting...")]
    public static partial void StartingTelegramUpdatePolling(this ILogger logger);

    [LoggerMessage(
        EventId = 1_0001_0002,
        Level = LogLevel.Information,
        Message = "Stopping Telegram polling because application is shutting down.")]
    public static partial void TelegramUpdatePollingStopped(this ILogger logger);

    [LoggerMessage(
        EventId = 1_0001_0003,
        Level = LogLevel.Critical,
        Message = "Telegram polling worked encountered an exception")]
    public static partial void TelegramUpdatePollingWorkerCrashed(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1_0001_0004,
        Level = LogLevel.Error,
        Message = "Failed to poll Telegram for updates")]
    public static partial void TelegramUpdatePollingFailed(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1_0001_0005,
        Level = LogLevel.Information,
        Message = "Received Telegram update. Update ID: {UpdateId}. Payload: {UpdatePayload}")]
    public static partial void TelegramUpdateReceived(this ILogger logger, long updateId, string updatePayload);

    [LoggerMessage(
        EventId = 1_0001_0006,
        Level = LogLevel.Error,
        Message = "Failed to handle telegram update. Update ID: {UpdateId}. Reason: {Reason}")]
    public static partial void TelegramUpdateHandlingFailed(this ILogger logger, long updateId, string reason);

    [LoggerMessage(
        EventId = 1_0001_0007,
        Level = LogLevel.Error,
        Message = "Failed to handle telegram update. Update ID: {UpdateId}")]
    public static partial void TelegramUpdateHandlingFailed(this ILogger logger, long updateId, Exception ex);

    [LoggerMessage(
        EventId = 1_0001_0008,
        Level = LogLevel.Warning,
        Message = "Failed to determine handler for a command. Message text: \"{MessageText}\"")]
    public static partial void FailedToDetermineCommandHandler(this ILogger logger, string? messageText);

    [LoggerMessage(
        EventId = 1_0001_0009,
        Level = LogLevel.Warning,
        Message = "Failed to determine handler for a callback query. Raw query data: \"{QueryData}\"")]
    public static partial void FailedToDetermineCallbackQueryHandler(this ILogger logger, string? queryData);

    #endregion
}
