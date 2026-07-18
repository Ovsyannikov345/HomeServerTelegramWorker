using HomeLabCore.Api.Constants;
using HomeLabCore.Api.Logging;
using HomeLabCore.Shared.Constants;
using HomeLabCore.Shared.Contexts;
using Serilog.Context;

namespace HomeLabCore.Api.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.Headers.TryGetValue(HeaderNames.CorrelationId, out var receivedCorrelationIds);

        var receivedCorrelationId = receivedCorrelationIds.FirstOrDefault();

        string correlationId;

        if (string.IsNullOrWhiteSpace(receivedCorrelationId) || !Guid.TryParse(receivedCorrelationId, out _))
        {
            correlationId = CorrelationContext.CorrelationId;

            using(LogContext.PushProperty(LogPropertyNames.CorrelationId, receivedCorrelationId))
            {
                logger.GeneratedNewRequestCorrelationId(receivedCorrelationId, correlationId);
            }
        }
        else
        {
            correlationId = receivedCorrelationId;
            CorrelationContext.CorrelationId = correlationId;
        }

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(HeaderNames.CorrelationId))
            {
                context.Response.Headers.Append(HeaderNames.CorrelationId, correlationId);
            }

            return Task.CompletedTask;
        });

        using (LogContext.PushProperty(LogPropertyNames.CorrelationId, correlationId))
        {
            await next(context);
        }
    }
}
