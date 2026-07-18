using HomeLabCore.Application.Interfaces.Clients;
using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Abstractions;
using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Payloads;
using HomeLabCore.Application.Telegram.Configuration;
using HomeLabCore.Application.Telegram.Constants;
using HomeLabCore.Application.Telegram.Exceptions;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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

    protected override async Task ProcessCallbackQuery(CallbackQueryContext context, RequestMediaPayload payload, CancellationToken ct)
    {
        var isSuccess = await mediaManagerClient.RequestMedia(payload.MediaType, payload.MediaId, ct);

        if (isSuccess)
        {
            var keyboard = context.SourceMessage.ReplyMarkup;

            if (keyboard is null)
            {
                return;
            }

            IEnumerable<IEnumerable<InlineKeyboardButton>> updatedKeyboard = [
                [ new InlineKeyboardButton("✅ Requested", new EmptyPayload().ToCallbackQueryString())],
                ..keyboard.InlineKeyboard.Skip(1)
            ];

            await UpdateMessageKeyboard(new InlineKeyboardMarkup(updatedKeyboard), ct);
        }
        else
        {
            throw new CallbackQueryProcessingException("Failed to request the media from Seerr.", showToUser: true);
        }
    }
}
