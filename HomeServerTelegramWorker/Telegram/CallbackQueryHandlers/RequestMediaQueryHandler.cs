using HomeServerTelegramWorker.Seerr;
using HomeServerTelegramWorker.Telegram.CallbackQueryHandlers.Payloads;
using HomeServerTelegramWorker.Telegram.Constants;
using HomeServerTelegramWorker.Telegram.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HomeServerTelegramWorker.Telegram.CallbackQueryHandlers;

// TODO add logging
public sealed class RequestMediaQueryHandler(
    ITelegramBotClient telegramBotClient,
    ISeerrClient seerrClient) 
    : CallbackQueryHandlerBase<RequestMediaPayload>(telegramBotClient)
{
    protected override string QueryPrefix => CallbackQueryConstants.Prefixes.RequestMedia;

    protected override async Task ProcessCallbackQuery(CallbackQuery callbackQuery, RequestMediaPayload payload, CancellationToken ct)
    {
        var isSuccess = await seerrClient.RequestMedia(payload.MediaType, payload.MediaId, ct);

        if (isSuccess)
        {
            var caption = callbackQuery.Message!.Caption ?? callbackQuery.Message.Text;
            var updatedCaption = caption + "\n\n✅ <b>Successfully requested!</b>";

            if (callbackQuery.Message.Type == MessageType.Photo)
            {
                await telegramBotClient.EditMessageCaption(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    caption: updatedCaption,
                    parseMode: ParseMode.Html,
                    replyMarkup: null, // TODO keep season buttons if series
                    cancellationToken: ct);
            }
            else
            {
                await telegramBotClient.EditMessageText(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: updatedCaption,
                    parseMode: ParseMode.Html,
                    replyMarkup: null, // TODO keep season buttons if series
                    cancellationToken: ct);
            }
        }
        else
        {
            throw new CallbackQueryProcessingException("Failed to request the media from Seerr.");
        }
    }
}
