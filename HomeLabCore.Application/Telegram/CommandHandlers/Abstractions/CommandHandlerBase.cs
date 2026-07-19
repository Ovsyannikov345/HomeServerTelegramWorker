using HomeLabCore.Application.Logging;
using HomeLabCore.Application.Telegram.Configuration;
using HomeLabCore.Application.Telegram.Dto;
using HomeLabCore.Application.Telegram.Exceptions;
using HomeLabCore.Shared.Contexts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HomeLabCore.Application.Telegram.CommandHandlers.Abstractions;

public interface ICommandHandler
{
    public CommandHandlerOptions HandlerOptions { get; }

    public bool CanHandle(Message message);

    public Task Handle(Message message, CancellationToken ct);
}

internal abstract class CommandHandlerBase(
    ITelegramBotClient telegramBotClient,
    IOptionsSnapshot<TelegramSettings> options,
    ILogger logger)
    : ICommandHandler
{
    protected readonly ITelegramBotClient BotClient = telegramBotClient;

    protected readonly TelegramSettings Settings = options.Value;

    protected readonly ILogger Logger = logger;

    public abstract CommandHandlerOptions HandlerOptions { get; }

    private CommandContext? _context;

    public virtual bool CanHandle(Message message)
    {
        if (message.Text is null
            || !message.Text.StartsWith('/'))
        {
            return false;
        }

        var parts = message.Text[1..].Split(' ');

        // Remove bot name if present (e.g., /command@botname)
        var command = parts[0].Split('@')[0];

        if (!string.Equals(command, HandlerOptions.CommandName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    public async Task Handle(Message message, CancellationToken ct)
    {
        Message? botResponseMessage = null;

        try
        {
            var userId = message.From?.Id;

            var hasAccess = userId is not null && Settings.UserIdWhitelist.Contains(userId.Value);

            if (HandlerOptions.RequiresAuthorization && !hasAccess)
            {
                Logger.CommandAccessDenied(userId, HandlerOptions.CommandName);

                await BotClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"❌ **Access denied.**",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: ct);

                return;
            }

            botResponseMessage = await BotClient.SendMessage(
                chatId: message.Chat.Id,
                text: "⏳ **Processing request...**",
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);

            _context = new CommandContext
            {
                UserMessage = message,
                BotResponseMessage = botResponseMessage
            };

            Logger.CommandProcessingStarted(message.Text, userId, message.Chat.Id);

            await ProcessUpdate(message, botResponseMessage, ct);

            Logger.CommandProcessingFinished();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await HandleException(message, botResponseMessage, ex, ct);
        }
    }

    protected abstract Task ProcessUpdate(Message message, Message botResponseMessage, CancellationToken ct);

    protected static string? GetCommandArgument(Message message)
    {
        var parts = message.Text?.Split(' ', 2);

        if (parts is null || parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
        {
            return null;
        }

        return parts[1];
    }

    protected async Task RespondWithText(string text, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(_context);

        await BotClient.EditMessageText(
            chatId: _context.BotResponseMessage.Chat.Id,
            messageId: _context.BotResponseMessage.MessageId,
            text: text,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);

        Logger.UpdatedTextForMessage(_context.BotResponseMessage.MessageId);
    }

    protected async Task RespondWithMessage(TelegramMessage message, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(_context);

        if (message.Photo is not null)
        {
            await BotClient.SendPhoto(
                chatId: _context.UserMessage.Chat.Id,
                photo: message.Photo,
                caption: message.Caption,
                parseMode: ParseMode.Markdown,
                replyMarkup: message.Keyboard,
                cancellationToken: ct);
        }
        else
        {
            await BotClient.SendMessage(
                chatId: _context.UserMessage.Chat.Id,
                text: message.Caption,
                parseMode: ParseMode.Markdown,
                replyMarkup: message.Keyboard,
                cancellationToken: ct);
        }

        Logger.RespondedToCommandWithNewMessage(_context.UserMessage.Chat.Id);

        await BotClient.DeleteMessage(
            chatId: _context.BotResponseMessage.Chat.Id,
            messageId: _context.BotResponseMessage.MessageId,
            cancellationToken: CancellationToken.None);

        Logger.DeletedMessage(_context.BotResponseMessage.MessageId);
    }

    private async Task HandleException(Message originalMessage, Message? botResponseMessage, Exception ex, CancellationToken ct)
    {
        if (ex is CommandProcessingException)
        {
            Logger.CommandProcessingFailed(ex.Message);
        }
        else
        {
            Logger.CommandProcessingFailed(ex);
        }

        var responseMessage = ex switch
        {
            CommandProcessingException commandEx when commandEx.ShowMessageToUser
                => $"❌ **{ex.Message}**",

            CommandProcessingException commandEx when !commandEx.ShowMessageToUser
                => $"❌ **Something went wrong while processing the command :(** \n\n🔍 Correlation ID: {CorrelationContext.CorrelationId}",

            _ => $"❌ **Something went wrong while processing the command :(** \n\n🔍 Correlation ID: {CorrelationContext.CorrelationId}"
        };

        if (botResponseMessage is null)
        {
            await BotClient.SendMessage(
                chatId: originalMessage.Chat.Id,
                text: responseMessage,
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
        }
        else
        {
            await BotClient.EditMessageText(
                chatId: botResponseMessage.Chat.Id,
                messageId: botResponseMessage.MessageId,
                text: responseMessage,
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
        }
    }
}
