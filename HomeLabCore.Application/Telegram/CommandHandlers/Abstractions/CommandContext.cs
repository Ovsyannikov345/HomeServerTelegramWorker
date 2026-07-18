using Telegram.Bot.Types;

namespace HomeLabCore.Application.Telegram.CommandHandlers.Abstractions;

public sealed record CommandContext
{
    public required Message UserMessage { get; init; }

    public required Message BotResponseMessage { get; init; }
}
