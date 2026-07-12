using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Payloads;
using HomeLabCore.Application.Telegram.Configuration;
using HomeLabCore.Application.Telegram.Exceptions;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HomeLabCore.Application.Telegram.CallbackQueryHandlers;

public interface ICallbackQueryHandler
{
    public bool CanHandle(CallbackQuery callbackQuery);

    public Task Handle(CallbackQuery callbackQuery, CancellationToken ct);
}

internal abstract class CallbackQueryHandlerBase<TPayload>(
    ITelegramBotClient telegramBotClient, 
    IOptionsSnapshot<TelegramSettings> options) 
    : ICallbackQueryHandler 
    where TPayload : ICallbackQueryPayload<TPayload>
{
    protected readonly ITelegramBotClient BotClient = telegramBotClient;

    protected readonly TelegramSettings Settings = options.Value;

    protected abstract bool RequiresAuthorization { get; }

    protected abstract string QueryPrefix { get; }

    public bool CanHandle(CallbackQuery callbackQuery)
    {
        return callbackQuery.Data is not null
            && callbackQuery.Data.StartsWith(QueryPrefix);
    }

    public async Task Handle(CallbackQuery callbackQuery, CancellationToken ct)
    {
        // TODO add logging
        try
        {
            var hasAccess = callbackQuery.From?.Id is { } userId && Settings.UserIdWhitelist.Contains(userId);

            if (RequiresAuthorization && !hasAccess)
            {
                await BotClient.AnswerCallbackQuery(callbackQuery.Id, "Access denied.", cancellationToken: ct);

                return;
            }

            await BotClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

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
            var errorMessage = ex.ShowMessageToUser
                ? ex.Message
                : "Something went wrong :(";

            if (callbackQuery.Message is { } message)
            {
                var caption = callbackQuery.Message!.Caption ?? callbackQuery.Message.Text;

                var updatedCaption = caption + $"\n\n❌ <b>{errorMessage}</b>";

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

    protected abstract Task ProcessCallbackQuery(CallbackQuery callbackQuery, TPayload payload, CancellationToken ct);
}
