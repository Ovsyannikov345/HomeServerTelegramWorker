using HomeServerTelegramWorker.Telegram.Constants;
using System.Diagnostics.CodeAnalysis;

namespace HomeServerTelegramWorker.Telegram.CallbackQueryHandlers.Payloads;

public sealed record RequestMediaPayload : ICallbackQueryPayload<RequestMediaPayload>
{
    public required string MediaType { get; init; }

    public required int MediaId { get; init; }

    public static bool TryParse(string data, [NotNullWhen(true)] out RequestMediaPayload? payload)
    {
        payload = null;

        var parts = data.Split(CallbackQueryConstants.Delimiter);

        if (parts.Length == 3
            && parts[0] == CallbackQueryConstants.Prefixes.RequestMedia
            && int.TryParse(parts[2], out var id))
        {
            payload = new RequestMediaPayload
            {
                MediaType = parts[1],
                MediaId = id
            };

            return true;
        }

        return false;
    }

    public string ToCallbackQueryString()
    {
        var prefix = CallbackQueryConstants.Prefixes.RequestMedia;

        var delimiter = CallbackQueryConstants.Delimiter;

        return $"{prefix}{delimiter}{MediaType}{delimiter}{MediaId}";
    }
}
