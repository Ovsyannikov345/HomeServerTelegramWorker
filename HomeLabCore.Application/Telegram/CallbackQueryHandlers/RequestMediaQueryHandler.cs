using HomeLabCore.Application.Interfaces.Clients;
using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Payloads;
using HomeLabCore.Application.Telegram.Configuration;
using HomeLabCore.Application.Telegram.Constants;
using HomeLabCore.Application.Telegram.Exceptions;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HomeLabCore.Application.Telegram.CallbackQueryHandlers;

// TODO add logging
internal sealed class RequestMediaQueryHandler(
    ITelegramBotClient telegramBotClient,
    IMediaManagerClient mediaManagerClient,
    IOptionsSnapshot<TelegramSettings> options) 
    : CallbackQueryHandlerBase<RequestMediaPayload>(telegramBotClient, options), ICallbackQueryHandler
{
    protected override string QueryPrefix => CallbackQueryConstants.Prefixes.RequestMedia;

    protected override bool RequiresAuthorization => true;

    protected override async Task ProcessCallbackQuery(CallbackQuery callbackQuery, RequestMediaPayload payload, CancellationToken ct)
    {
        var isSuccess = await mediaManagerClient.RequestMedia(payload.MediaType, payload.MediaId, ct);

        if (isSuccess)
        {
            var caption = callbackQuery.Message!.Caption ?? callbackQuery.Message.Text;
            var updatedCaption = caption + "\n\n✅ <b>Successfully requested!</b>";

            if (callbackQuery.Message.Type == MessageType.Photo)
            {
                await BotClient.EditMessageCaption(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    caption: updatedCaption,
                    parseMode: ParseMode.Html,
                    replyMarkup: null, // TODO keep season buttons if series
                    cancellationToken: ct);
            }
            else
            {
                await BotClient.EditMessageText(
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
            throw new CallbackQueryProcessingException("Failed to request the media from Seerr.", showToUser: true);
        }
    }
}
