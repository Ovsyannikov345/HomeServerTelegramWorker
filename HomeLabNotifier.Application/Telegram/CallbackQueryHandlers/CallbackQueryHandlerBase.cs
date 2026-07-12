using HomeLabNotifier.Application.Telegram.CallbackQueryHandlers.Payloads;
using HomeLabNotifier.Application.Telegram.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramTypes = Telegram.Bot.Types;

namespace HomeLabNotifier.Application.Telegram.CallbackQueryHandlers;

public interface ICallbackQueryHandler
{
    public bool CanHandle(TelegramTypes.CallbackQuery callbackQuery);

    public Task Handle(TelegramTypes.CallbackQuery callbackQuery, CancellationToken ct);
}

public abstract class CallbackQueryHandlerBase<TPayload>(ITelegramBotClient telegramBotClient) : ICallbackQueryHandler
    where TPayload : ICallbackQueryPayload<TPayload>
{
    protected readonly ITelegramBotClient BotClient = telegramBotClient;

    protected abstract string QueryPrefix { get; }

    public bool CanHandle(TelegramTypes.CallbackQuery callbackQuery)
    {
        return callbackQuery.Data is not null
            && callbackQuery.Data.StartsWith(QueryPrefix);
    }

    public async Task Handle(TelegramTypes.CallbackQuery callbackQuery, CancellationToken ct)
    {
        // TODO add logging
        try
        {
            await BotClient.AnswerCallbackQuery(callbackQuery.Id, "Processing...", cancellationToken: ct);

            if (callbackQuery.Data is null)
            {
                return;
            }

            if (!TPayload.TryParse(callbackQuery.Data, out var payload))
            {
                return;
            }

            await ProcessCallbackQuery(callbackQuery, payload, ct);
        }
        // TODO add correrlation id to message for debugging
        catch (CallbackQueryProcessingException ex)
        {
            if (callbackQuery.Message is { } message)
            {
                var caption = callbackQuery.Message!.Caption ?? callbackQuery.Message.Text;

                var updatedCaption = caption + $"\n\n❌ <b>{ex.Message}</b>";

                await BotClient.EditMessageCaption(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    caption: updatedCaption,
                    parseMode: ParseMode.Html,
                    replyMarkup: null,
                    cancellationToken: ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (callbackQuery.Message is { } message)
            {
                var caption = callbackQuery.Message!.Caption ?? callbackQuery.Message.Text;

                var updatedCaption = caption + "\n\n❌ <b>Something went wrong :(</b>";

                await BotClient.EditMessageCaption(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    caption: updatedCaption,
                    parseMode: ParseMode.Html,
                    replyMarkup: null,
                    cancellationToken: ct);
            }
        }
    }

    protected abstract Task ProcessCallbackQuery(TelegramTypes.CallbackQuery callbackQuery, TPayload payload, CancellationToken ct);
}
