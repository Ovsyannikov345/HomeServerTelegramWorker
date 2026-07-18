using System.Diagnostics.CodeAnalysis;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HomeLabCore.Worker.TelegramPolling.Extensions;

internal static class TelegramUpdateExtensions
{
    public static bool TryExtractCommand(this Update update, [NotNullWhen(true)] out Message? command)
    {
        command = null;

        if (update.Type is UpdateType.Message
            && update.Message?.Text is not null
            && update.Message.Text.StartsWith('/'))
        {
            command = update.Message;

            return true;
        }

        return false;
    }

    public static bool TryExtractCallbackQuery(this Update update, [NotNullWhen(true)] out CallbackQuery? callbackQuery)
    {
        callbackQuery = null;

        if (update.Type is UpdateType.CallbackQuery && update.CallbackQuery is not null)
        {
            callbackQuery = update.CallbackQuery;

            return true;
        }

        return false;
    }
}
