using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HomeServerTelegramWorker.Telegram.CommandHandlers;

public sealed class HelpCommandHandler(
    ITelegramBotClient telegramBotClient,
    IServiceScopeFactory serviceScopeFactory)
    : CommandHandlerBase, ICommandHandler
{
    public override string CommandName => "help";

    public override string CommandDescription => "Prints a list of commands that the bot can execute";

    public override string? CommandExample => null;

    protected override async Task ProcessUpdate(Message message, CancellationToken ct)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();

        var commandHandlers = scope.ServiceProvider.GetServices<ICommandHandler>();

        var sb = new StringBuilder();

        sb.AppendLine("👋 <b>Hi there! I'm your home server bot.</b>\n");
        sb.AppendLine("I run in the background to automatically send you updates and alerts from our server services.\n");
        sb.AppendLine("You can also control things directly. Here is what I can do for you:\n");

        foreach (var handler in commandHandlers)
        {
            sb.AppendLine($"🔹 <b>/{handler.CommandName}</b> — {handler.CommandDescription}");

            if (!string.IsNullOrWhiteSpace(handler.CommandExample))
            {
                sb.AppendLine($"    ↳ 💡 <i>Example:</i> <code>{handler.CommandExample}</code>");
            }

            sb.AppendLine();
        }

        await telegramBotClient.SendMessage(
            chatId: message.Chat.Id,
            text: sb.ToString(),
            parseMode: ParseMode.Html,
            cancellationToken: ct);
    }
}
