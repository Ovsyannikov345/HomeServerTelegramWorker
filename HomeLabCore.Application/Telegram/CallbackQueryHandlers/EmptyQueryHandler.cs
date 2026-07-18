using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Payloads;
using HomeLabCore.Application.Telegram.Configuration;
using HomeLabCore.Application.Telegram.Constants;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace HomeLabCore.Application.Telegram.CallbackQueryHandlers;

internal sealed class EmptyQueryHandler(
    ITelegramBotClient telegramBotClient,
    IOptionsSnapshot<TelegramSettings> options)
    : CallbackQueryHandlerBase<RequestMediaPayload>(telegramBotClient, options)
{
    protected override bool RequiresAuthorization => true;

    protected override string QueryPrefix => CallbackQueryConstants.Prefixes.Empty;

    protected override Task ProcessCallbackQuery(CallbackQueryContext context, RequestMediaPayload payload, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
