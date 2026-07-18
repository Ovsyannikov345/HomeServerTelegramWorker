using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HomeLabCore.Application.Telegram.CallbackQueryHandlers;

public interface IFallbackCallbackQueryHandler
{
    public Task Handle(CallbackQuery callbackQuery, CancellationToken ct);
}

internal class FallbackCallbackQueryHandler(ITelegramBotClient telegramBotClient, ILogger<FallbackCallbackQueryHandler> logger) 
    : IFallbackCallbackQueryHandler
{
    public async Task Handle(CallbackQuery callbackQuery, CancellationToken ct)
    {
        logger.LogWarning("No handler found for callback query {CallbackQueryId}. Sending fallback message.", callbackQuery.Id);

        await telegramBotClient.AnswerCallbackQuery(callbackQuery.Id);

        if (callbackQuery.Message is { } message)
        {
            await telegramBotClient.SendMessage(
                chatId: message.Chat.Id,
                text: "Sorry, I don't know how to handle that action.",
                cancellationToken: ct
            );
        }
    }
}
