using HomeLabCore.Application.Telegram.Configuration;
using HomeLabCore.Application.Telegram.Dto;
using HomeLabCore.Application.Telegram.Exceptions;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HomeLabCore.Application.Telegram.CommandHandlers;

public interface ICommandHandler
{
    public CommandHandlerOptions HandlerOptions { get; }

    public bool CanHandle(Message message);

    public Task Handle(Message message, CancellationToken ct);
}

internal abstract class CommandHandlerBase(
    ITelegramBotClient telegramBotClient,
    IOptionsSnapshot<TelegramSettings> options)
    : ICommandHandler
{
    protected readonly ITelegramBotClient BotClient = telegramBotClient;

    protected readonly TelegramSettings Settings = options.Value;

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

    // TODO logs here
    public async Task Handle(Message message, CancellationToken ct)
    {
        Message? botResponseMessage = null;

        try
        {
            var hasAccess = message.From?.Id is { } userId && Settings.UserIdWhitelist.Contains(userId);

            if (HandlerOptions.RequiresAuthorization && !hasAccess)
            {
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

            await ProcessUpdate(message, botResponseMessage, ct);
        }
        // TODO add correrlation id to message for debugging
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
        ArgumentNullException.ThrowIfNull(_context, nameof(_context));

        await BotClient.EditMessageText(
            chatId: _context.BotResponseMessage.Chat.Id,
            messageId: _context.BotResponseMessage.MessageId,
            text: text,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    protected async Task RespondWithMessage(TelegramMessage message, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(_context, nameof(_context));

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

        await BotClient.DeleteMessage(
            chatId: _context.BotResponseMessage.Chat.Id,
            messageId: _context.BotResponseMessage.MessageId,
            cancellationToken: CancellationToken.None);
    }

    private async Task HandleException(Message originalMessage, Message? botResponseMessage, Exception ex, CancellationToken ct)
    {
        var responseMessage = ex switch
        {
            CommandProcessingException commandEx when commandEx.ShowMessageToUser
                => $"❌ **{ex.Message}**",

            CommandProcessingException commandEx when !commandEx.ShowMessageToUser
                => $"❌ **Something went wrong while processing the command :(**",

            _ => $"❌ **Something went wrong while processing the command :(**"
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
