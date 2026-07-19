namespace HomeLabCore.Api.Logging;

internal static partial class ApplicationLogs
{
    #region Middleware

    [LoggerMessage(
        EventId = 4_0001_0001,
        Level = LogLevel.Information,
        Message = "Received correlation ID '{ReceivedCorrelationId}' is empty or invalid. Generated new correlation ID: '{GeneratedCorrelationId}'")]
    public static partial void GeneratedNewRequestCorrelationId(this ILogger logger, string? receivedCorrelationId, string generatedCorrelationId);

    #endregion
}
