using HomeLabCore.Application.Telegram.Configuration;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HomeLabCore.Application.Telegram.CommandHandlers;

internal sealed class StartCommandHandler(
    ITelegramBotClient telegramBotClient,
    IOptionsSnapshot<TelegramSettings> options)
    : CommandHandlerBase(telegramBotClient, options)
{
    public override bool RequiresAuthorization => false;

    public override string CommandName => "start";

    public override string CommandDescription => "Starts a conversation with the bot";

    public override string? CommandExample => $"/{CommandName}";

    protected override async Task ProcessUpdate(Message message, Message botResponseMessage, CancellationToken ct)
    {
        var userId = message.From?.Id;

        var chatId = message.Chat.Id;

        var firstName = message.From?.FirstName ?? "anonymous user";

        if (userId is not null && Settings.UserIdWhitelist.Contains(userId.Value))
        {
            var alreadyWhitelistedText = $"""
            👋 <b>Welcome, {firstName}!</b>

            You're already whitelisted. Anyways, here are your ID's:

            👤 <b>User ID:</b> <code>{userId}</code>
            💬 <b>Chat ID:</b> <code>{chatId}</code>

            <i>Use /help command to get started.</i>
            """;

            await BotClient.EditMessageText(
                chatId: botResponseMessage.Chat.Id,
                messageId: botResponseMessage.Id,
                text: alreadyWhitelistedText,
                parseMode: ParseMode.Html,
                cancellationToken: ct);

            return;
        }

        var greetingText = $"""
        👋 <b>Welcome, {firstName}!</b>

        This is a private home server bot. To use it, you must be authorized by the administrator.

        Please tap the IDs below to copy them, and send them to the admin to be whitelisted:

        👤 <b>User ID:</b> <code>{userId}</code>
        💬 <b>Chat ID:</b> <code>{chatId}</code>

        <i>Once the admin confirms you are added, you can start talking to me!</i>
        <i>Use /help command to get started.</i>
        """;

        await BotClient.EditMessageText(
            chatId: chatId,
            messageId: botResponseMessage.Id,
            text: greetingText,
            parseMode: ParseMode.Html,
            cancellationToken: ct);
    }
}
