using HomeLabCore.Application.Interfaces.Clients;
using HomeLabCore.Application.Logging;
using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Abstractions;
using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Payloads;
using HomeLabCore.Application.Telegram.Configuration;
using HomeLabCore.Application.Telegram.Constants;
using HomeLabCore.Application.Telegram.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace HomeLabCore.Application.Telegram.CallbackQueryHandlers;

internal sealed class RequestMediaQueryHandler(
    ITelegramBotClient telegramBotClient,
    IMediaManagerClient mediaManagerClient,
    IOptionsSnapshot<TelegramSettings> options,
    ILogger<RequestMediaQueryHandler> logger)
    : CallbackQueryHandlerBase<RequestMediaPayload>(telegramBotClient, options, logger)
{
    protected override string QueryPrefix => CallbackQueryConstants.Prefixes.RequestMedia;

    protected override bool RequiresAuthorization => true;

    protected override async Task ProcessCallbackQuery(CallbackQueryContext context, RequestMediaPayload payload, CancellationToken ct)
    {
        try
        {
            await mediaManagerClient.RequestMedia(payload.MediaType, payload.MediaId, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.FailedToRequestMedia(payload.MediaType, payload.MediaId, ex);

            throw new CallbackQueryProcessingException("Failed to request media", showToUser: true);
        }

        Logger.RequestedMedia(payload.MediaType, payload.MediaId);

        var keyboard = context.SourceMessage.ReplyMarkup;

        if (keyboard is null)
        {
            return;
        }

        IEnumerable<IEnumerable<InlineKeyboardButton>> updatedKeyboard =
        [
            [ new InlineKeyboardButton("✅ Requested", new EmptyPayload().ToCallbackQueryString())],
            ..keyboard.InlineKeyboard.Skip(1)
        ];

        await UpdateMessageKeyboard(new InlineKeyboardMarkup(updatedKeyboard), ct);
    }
}
