using HomeLabCore.Application.Telegram.Configuration;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HomeLabCore.Application.Telegram.CommandHandlers;

internal sealed class StartCommandHandler(
    ITelegramBotClient telegramBotClient,
    IOptionsSnapshot<TelegramSettings> options)
    : CommandHandlerBase(telegramBotClient, options)
{
    public override CommandHandlerOptions HandlerOptions => new()
    {
        RequiresAuthorization = false,
        CommandName = "start",
        CommandDescription = "Starts a conversation with the bot",
        CommandExample = null
    };

    protected override async Task ProcessUpdate(Message message, Message botResponseMessage, CancellationToken ct)
    {
        var userId = message.From?.Id;

        var chatId = message.Chat.Id;

        var firstName = message.From?.FirstName ?? "anonymous user";

        if (userId is not null && Settings.UserIdWhitelist.Contains(userId.Value))
        {
            var alreadyWhitelistedText = $"""
            👋 **Welcome, {firstName}!**

            You're already whitelisted. Anyways, here are your ID's:

            👤 **User ID:** `{userId}`
            💬 **Chat ID:** `{chatId}`

            *Use /help command to get started.*
            """;

            await RespondWithText(alreadyWhitelistedText, ct);

            return;
        }

        var greetingText = $"""
        👋 **Welcome, {firstName}!**

        This is a private home server bot. To use it, you must be authorized by the administrator.

        Please tap the IDs below to copy them, and send them to the admin to be whitelisted:

        👤 **User ID:** `{userId}`
        💬 **Chat ID:** `{chatId}`

        *Once the admin confirms you are added, you can start talking to me!*
        *Use /help command to get started.*
        """;

        await RespondWithText(greetingText, ct);
    }
}
