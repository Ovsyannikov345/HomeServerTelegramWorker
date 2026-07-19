using Telegram.Bot;
using Telegram.Bot.Types;

namespace HomeLabCore.Application.Telegram.CallbackQueryHandlers;

public interface IFallbackCallbackQueryHandler
{
    public Task Handle(CallbackQuery callbackQuery, CancellationToken ct);
}

internal class FallbackCallbackQueryHandler(ITelegramBotClient telegramBotClient)
    : IFallbackCallbackQueryHandler
{
    public async Task Handle(CallbackQuery callbackQuery, CancellationToken ct)
    {
        await telegramBotClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

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
