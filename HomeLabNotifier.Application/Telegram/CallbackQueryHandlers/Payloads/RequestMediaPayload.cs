using HomeLabNotifier.Application.Telegram.Constants;
using HomeLabNotifier.Domain.Constants.Enums;
using System.Diagnostics.CodeAnalysis;

namespace HomeLabNotifier.Application.Telegram.CallbackQueryHandlers.Payloads;

public sealed record RequestMediaPayload : ICallbackQueryPayload<RequestMediaPayload>
{
    public required MediaType MediaType { get; init; }

    public required int MediaId { get; init; }

    public static bool TryParse(string data, [NotNullWhen(true)] out RequestMediaPayload? payload)
    {
        payload = null;

        var parts = data.Split(CallbackQueryConstants.Delimiter);

        if (parts.Length == 3
            && parts[0] == CallbackQueryConstants.Prefixes.RequestMedia
            && Enum.TryParse(parts[1], out MediaType type)
            && int.TryParse(parts[2], out var id))
        {
            payload = new RequestMediaPayload
            {
                MediaType = type,
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
