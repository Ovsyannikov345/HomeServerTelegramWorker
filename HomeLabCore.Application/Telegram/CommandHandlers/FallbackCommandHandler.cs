using Telegram.Bot;
using Telegram.Bot.Types;

namespace HomeLabCore.Application.Telegram.CommandHandlers;

public interface IFallbackCommandHandler
{
    public Task Handle(Message message, CancellationToken ct);
}

public sealed class FallbackCommandHandler(ITelegramBotClient telegramBotClient) : IFallbackCommandHandler
{
    public async Task Handle(Message message, CancellationToken ct)
    {
        await telegramBotClient.SendMessage(
            chatId: message.Chat.Id,
            text: "Sorry, I don't know how to handle that. Use /help command to see the list of available commands.",
            cancellationToken: ct
        );
    }
}
