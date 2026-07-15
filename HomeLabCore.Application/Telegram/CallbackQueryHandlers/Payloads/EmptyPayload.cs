using HomeLabCore.Application.Telegram.Constants;
using System.Diagnostics.CodeAnalysis;

namespace HomeLabCore.Application.Telegram.CallbackQueryHandlers.Payloads;

public sealed record EmptyPayload : ICallbackQueryPayload<EmptyPayload>
{
    public static bool TryParse(string data, [NotNullWhen(true)] out EmptyPayload? payload)
    {
        payload = null;

        var parts = data.Split(CallbackQueryConstants.Delimiter);

        if (parts.Length == 1 && parts[0] == CallbackQueryConstants.Prefixes.Empty)
        {
            payload = new EmptyPayload();

            return true;
        }

        return false;
    }

    public string ToCallbackQueryString()
    {
        return CallbackQueryConstants.Prefixes.Empty;
    }
}
