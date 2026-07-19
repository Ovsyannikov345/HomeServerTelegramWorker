using Microsoft.Extensions.Logging;

namespace HomeLabCore.Infrastructure.Logging;

internal static partial class ApplicationLogs
{
    #region Database

    [LoggerMessage(
        EventId = 2_0001_0001,
        Level = LogLevel.Information,
        Message = "Applying pending database migrations...")]
    public static partial void MigratingDatabase(this ILogger logger);

    [LoggerMessage(
        EventId = 2_0001_0002,
        Level = LogLevel.Information,
        Message = "Database migration completed successfully")]
    public static partial void MigratedDatabaseSuccessfully(this ILogger logger);

    [LoggerMessage(
        EventId = 2_0001_0003,
        Level = LogLevel.Error,
        Message = "Database migration failed during startup")]
    public static partial void DatabaseMigrationFailed(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 2_0001_0004,
        Level = LogLevel.Information,
        Message = "Creating new {EntityName}")]
    public static partial void CreatingNewEntity(this ILogger logger, string entityName);

    [LoggerMessage(
        EventId = 2_0001_0005,
        Level = LogLevel.Information,
        Message = "Updating {EntityName}. Changed properties: {Properties}")]
    public static partial void UpdatingEntity(this ILogger logger, string entityName, string properties);

    [LoggerMessage(
        EventId = 2_0001_0006,
        Level = LogLevel.Information,
        Message = "Deleting {EntityName}")]
    public static partial void DeletingEntity(this ILogger logger, string entityName);

    #endregion

    #region Seerr Integration

    [LoggerMessage(
        EventId = 2_0002_0001,
        Level = LogLevel.Warning,
        Message = "Call to Seerr API failed. Method: {Method}. Request URL: {Url}. Status code: {StatusCode}. " +
            "Request body: {RequestBody}. Response body: {ResponseBody}")]
    public static partial void SeerrApiCallFailed(this ILogger logger, HttpMethod method, string? url, int statusCode, string requestBody, string responseBody);

    [LoggerMessage(
        EventId = 2_0002_0002,
        Level = LogLevel.Warning,
        Message = "Call to Seerr API failed. Method: {Method}. Request URL: {Url}")]
    public static partial void SeerrApiCallFailed(this ILogger logger, HttpMethod method, string? url, Exception ex);

    #endregion
}
