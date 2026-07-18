using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Payloads;
using HomeLabCore.Application.Telegram.Configuration;
using HomeLabCore.Application.Telegram.Dto;
using HomeLabCore.Application.Telegram.Exceptions;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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

    private CallbackQueryContext? _context;

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

            if (callbackQuery.Message is null)
            {
                return;
            }

            _context = new CallbackQueryContext
            {
                SourceMessage = callbackQuery.Message
            };

            await ProcessCallbackQuery(_context, payload, ct);
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

                var updatedCaption = caption + $"\n\n❌ **{errorMessage}**";

                await BotClient.EditMessageCaption(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    caption: updatedCaption,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: null,
                    cancellationToken: ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (callbackQuery.Message is { } message)
            {
                var caption = callbackQuery.Message!.Caption ?? callbackQuery.Message.Text;

                var updatedCaption = caption + "\n\n❌ **Something went wrong :(**";

                await BotClient.EditMessageCaption(
                    chatId: message.Chat.Id,
                    messageId: message.MessageId,
                    caption: updatedCaption,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: null,
                    cancellationToken: ct);
            }
        }
    }

    protected abstract Task ProcessCallbackQuery(CallbackQueryContext context, TPayload payload, CancellationToken ct);

    protected async Task RespondWithMessage(TelegramMessage message, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(_context, nameof(_context));

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
    }

    protected async Task UpdateMessageKeyboard(InlineKeyboardMarkup keyboard, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(_context, nameof(_context));

        await BotClient.EditMessageReplyMarkup(
            _context.SourceMessage.Chat.Id,
            _context.SourceMessage.Id,
            keyboard,
            cancellationToken: ct);
    }

    protected async Task DeleteOriginalMessage(CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(_context, nameof(_context));

        await BotClient.DeleteMessage(
            chatId: _context.SourceMessage.Chat.Id,
            messageId: _context.SourceMessage.MessageId,
            cancellationToken: ct);
    }
}
