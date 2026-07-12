using HomeLabCore.Application.Telegram.Configuration;
using HomeLabCore.Application.Telegram.Exceptions;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HomeLabCore.Application.Telegram.CommandHandlers;

public interface ICommandHandler
{
    public string CommandName { get; }

    public string CommandDescription { get; }

    public string? CommandExample { get; }

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

    public abstract bool RequiresAuthorization { get; }

    public abstract string CommandName { get; }

    public abstract string CommandDescription { get; }

    public abstract string? CommandExample { get; }

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

        if (!string.Equals(command, CommandName, StringComparison.OrdinalIgnoreCase))
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

            if (RequiresAuthorization && !hasAccess)
            {
                await BotClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"❌ **Access denied.**",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: ct);

                return;
            }

            // TODO change icon
            // TODO this should be controlled by handlers. Sometimes there's no need for message
            botResponseMessage = await BotClient.SendMessage(
                chatId: message.Chat.Id,
                text: "🔍 **Processing request...**",
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);

            await ProcessUpdate(message, botResponseMessage, ct);
        }
        // TODO add correrlation id to message for debugging
        // TODO refactor
        catch (CommandProcessingException ex)
        {
            if (botResponseMessage is null)
            {
                await BotClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"Something went wrong while processing the command :(",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: ct);
            }
            else
            {
                var errorMessage = ex.ShowMessageToUser
                    ? $"❌ **{ex.Message}**"
                    : "❌ **Something went wrong while processing the command :(**";

                await BotClient.EditMessageText(
                    chatId: botResponseMessage.Chat.Id,
                    messageId: botResponseMessage.MessageId,
                    text: errorMessage,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (botResponseMessage is null)
            {
                await BotClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Something went wrong while processing the command :(",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: ct);
            }
            else
            {
                await BotClient.EditMessageText(
                    chatId: botResponseMessage.Chat.Id,
                    messageId: botResponseMessage.MessageId,
                    text: "❌ **Something went wrong while processing the command :(**",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: ct);
            }
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
}
