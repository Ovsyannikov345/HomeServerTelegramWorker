using HomeLabCore.Infrastructure.Logging;
using Microsoft.Extensions.Logging;

namespace HomeLabCore.Infrastructure.Seerr.Handlers;

internal sealed class SeerrFailedResponseLoggingHandler(ILogger<SeerrFailedResponseLoggingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        try
        {
            response = await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.SeerrApiCallFailed(request.Method, request.RequestUri?.ToString(), ex);

            throw;
        }

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            string requestBody = string.Empty;

            if (request.Content is not null)
            {
                requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            logger.SeerrApiCallFailed(request.Method, request.RequestUri?.ToString(), (int)response.StatusCode, requestBody, responseBody);
        }

        return response;
    }
}
