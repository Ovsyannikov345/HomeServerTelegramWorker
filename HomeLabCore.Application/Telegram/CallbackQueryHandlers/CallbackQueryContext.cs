using Telegram.Bot.Types;

namespace HomeLabCore.Application.Telegram.CallbackQueryHandlers;

public sealed record CallbackQueryContext
{
    public required Message SourceMessage { get; init; }
}
