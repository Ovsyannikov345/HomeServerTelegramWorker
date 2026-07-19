using Telegram.Bot.Types;

namespace HomeLabCore.Application.Telegram.CallbackQueryHandlers.Abstractions;

public sealed record CallbackQueryContext
{
    public required CallbackQuery CallbackQuery { get; init; }

    public required Message SourceMessage { get; init; }
}
