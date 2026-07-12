using System.Diagnostics.CodeAnalysis;

namespace HomeLabNotifier.Application.Telegram.CallbackQueryHandlers.Payloads;

public interface ICallbackQueryPayload<TSelf>
{
    public static abstract bool TryParse(string data, [NotNullWhen(true)] out TSelf? payload);

    public string ToCallbackQueryString();
}
