using Telegram.Bot;
using Telegram.Bot.Types;

namespace HomeServerTelegramWorker.Telegram;

public interface IFallbackHandler
{
    public Task Handle(Message message, CancellationToken ct);
}

// TODO move to better place
public sealed class FallbackHandler(ITelegramBotClient telegramBotClient, ILogger<FallbackHandler> logger) : IFallbackHandler
{
    public async Task Handle(Message message, CancellationToken ct)
    {
        logger.LogWarning("No handler found for message {MessageId}. Sending fallback message.", message.Id);

        await telegramBotClient.SendMessage(
            chatId: message.Chat.Id,
            text: "Sorry, I don't know how to handle that. Use /help command to see the list of available commands.",
            cancellationToken: ct
        );
    }
}
