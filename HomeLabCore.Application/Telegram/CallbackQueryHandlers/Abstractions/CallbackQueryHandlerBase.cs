using HomeLabCore.Application.Logging;
using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Payloads;
using HomeLabCore.Application.Telegram.Configuration;
using HomeLabCore.Application.Telegram.Dto;
using HomeLabCore.Application.Telegram.Exceptions;
using HomeLabCore.Shared.Contexts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HomeLabCore.Application.Telegram.CallbackQueryHandlers.Abstractions;

public interface ICallbackQueryHandler
{
    public bool CanHandle(CallbackQuery callbackQuery);

    public Task Handle(CallbackQuery callbackQuery, CancellationToken ct);
}

internal abstract class CallbackQueryHandlerBase<TPayload>(
    ITelegramBotClient telegramBotClient,
    IOptionsSnapshot<TelegramSettings> options,
    ILogger logger)
    : ICallbackQueryHandler
    where TPayload : ICallbackQueryPayload<TPayload>
{
    protected readonly ITelegramBotClient BotClient = telegramBotClient;

    protected readonly TelegramSettings Settings = options.Value;

    protected readonly ILogger Logger = logger;

    protected abstract bool RequiresAuthorization { get; }

    protected abstract string QueryPrefix { get; }

    private CallbackQueryContext? _context;

    public bool CanHandle(CallbackQuery callbackQuery)
    {
        return callbackQuery.Data is not null
            && callbackQuery.Data.StartsWith(QueryPrefix);
    }

    public async Task Handle(CallbackQuery callbackQuery, CancellationToken ct)
    {
        try
        {
            var userId = callbackQuery.From?.Id;

            var hasAccess = userId is not null && Settings.UserIdWhitelist.Contains(userId.Value);

            if (RequiresAuthorization && !hasAccess)
            {
                Logger.CallbackQueryAccessDenied(callbackQuery.From?.Id, QueryPrefix);

                await BotClient.AnswerCallbackQuery(callbackQuery.Id, "Access denied.", cancellationToken: ct);

                return;
            }

            await BotClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

            if (callbackQuery.Data is null || callbackQuery.Message is null)
            {
                Logger.CallbackQueryRejected(callbackQuery.Id, "Data or Message is null");

                return;
            }

            if (!TPayload.TryParse(callbackQuery.Data, out var payload))
            {
                Logger.CallbackQueryParsingFailed(callbackQuery.Data, typeof(TPayload).Name);

                return;
            }

            _context = new CallbackQueryContext
            {
                CallbackQuery = callbackQuery,
                SourceMessage = callbackQuery.Message
            };

            Logger.CallbackQueryProcessingStarted(callbackQuery.Data, userId);

            await ProcessCallbackQuery(_context, payload, ct);

            Logger.CallbackQueryProcessingFinished();
        }
        catch (CallbackQueryProcessingException ex)
        {
            Logger.CallbackQueryProcessingFailed(callbackQuery.Id, callbackQuery.From?.Id, ex.Message);

            var errorMessage = ex.ShowMessageToUser
                ? ex.Message
                : "Something went wrong :(";

            await ShowErrorOnMessage(callbackQuery.Message, errorMessage, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.CallbackQueryProcessingFailed(callbackQuery.Id, callbackQuery.From?.Id, ex);

            await ShowErrorOnMessage(callbackQuery.Message, "Something went wrong :(", ct);
        }
    }

    protected abstract Task ProcessCallbackQuery(CallbackQueryContext context, TPayload payload, CancellationToken ct);

    protected async Task RespondWithMessage(TelegramMessage message, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(_context);

        if (message.Photo is not null)
        {
            await BotClient.SendPhoto(
                chatId: _context.SourceMessage.Chat.Id,
                photo: message.Photo,
                caption: message.Caption,
                parseMode: ParseMode.Markdown,
                replyMarkup: message.Keyboard,
                cancellationToken: ct);
        }
        else
        {
            await BotClient.SendMessage(
                chatId: _context.SourceMessage.Chat.Id,
                text: message.Caption,
                parseMode: ParseMode.Markdown,
                replyMarkup: message.Keyboard,
                cancellationToken: ct);
        }

        Logger.RespondedToCallbackQueryWithNewMessage(_context.CallbackQuery.Id, _context.SourceMessage.Chat.Id);
    }

    protected async Task UpdateMessageKeyboard(InlineKeyboardMarkup keyboard, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(_context);

        await BotClient.EditMessageReplyMarkup(
            _context.SourceMessage.Chat.Id,
            _context.SourceMessage.Id,
            keyboard,
            cancellationToken: ct);

        Logger.UpdatedInlineKeyboardForMessage(_context.SourceMessage.Id);
    }

    protected async Task DeleteOriginalMessage(CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(_context);

        await BotClient.DeleteMessage(
            chatId: _context.SourceMessage.Chat.Id,
            messageId: _context.SourceMessage.MessageId,
            cancellationToken: ct);

        Logger.DeletedMessage(_context.SourceMessage.Id);
    }

    private async Task ShowErrorOnMessage(Message? message, string errorMessage, CancellationToken ct)
    {
        if (message is null)
        {
            return;
        }

        var caption = message.Caption ?? message.Text;
        var updatedCaption = caption + $"\n\n❌ **{errorMessage}** \n\n🔍 Correlation ID: {CorrelationContext.CorrelationId}";

        await BotClient.EditMessageCaption(
            chatId: message.Chat.Id,
            messageId: message.MessageId,
            caption: updatedCaption,
            parseMode: ParseMode.Markdown,
            replyMarkup: null,
            cancellationToken: ct);
    }
}
