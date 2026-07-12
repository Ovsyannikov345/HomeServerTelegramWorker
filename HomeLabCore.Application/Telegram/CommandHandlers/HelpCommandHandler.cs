using HomeLabCore.Application.Telegram.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HomeLabCore.Application.Telegram.CommandHandlers;

internal sealed class HelpCommandHandler(
    ITelegramBotClient telegramBotClient,
    IServiceScopeFactory serviceScopeFactory,
    IOptionsSnapshot<TelegramSettings> options)
    : CommandHandlerBase(telegramBotClient, options)
{
    public override bool RequiresAuthorization => true;

    public override string CommandName => "help";

    public override string CommandDescription => "Prints a list of commands that the bot can execute";

    public override string? CommandExample => null;

    protected override async Task ProcessUpdate(Message message, Message botResponseMessage, CancellationToken ct)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();

        var commandHandlers = scope.ServiceProvider
            .GetServices<ICommandHandler>()
            .Where(s => s is not StartCommandHandler);

        var sb = new StringBuilder();

        sb.AppendLine("👋 <b>Hi there! I'm your home server bot.</b>\n");
        sb.AppendLine("I run in the background to automatically send you updates and alerts from our server services.\n");
        sb.AppendLine("I am also capable of doing some cool things. Here is what I can do for you:\n");

        foreach (var handler in commandHandlers)
        {
            sb.AppendLine($"🔹 <b>/{handler.CommandName}</b> — {handler.CommandDescription}");

            if (!string.IsNullOrWhiteSpace(handler.CommandExample))
            {
                sb.AppendLine($"    ↳ 💡 <i>Example:</i> <code>{handler.CommandExample}</code>");
            }

            sb.AppendLine();
        }

        await BotClient.EditMessageText(
            chatId: botResponseMessage.Chat.Id,
            messageId: botResponseMessage.Id,
            text: sb.ToString(),
            parseMode: ParseMode.Html,
            cancellationToken: ct);
    }
}
