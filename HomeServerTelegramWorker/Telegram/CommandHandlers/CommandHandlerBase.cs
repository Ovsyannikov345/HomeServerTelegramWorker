using Telegram.Bot.Types;

namespace HomeServerTelegramWorker.Telegram.CommandHandlers;

public abstract class CommandHandlerBase : ICommandHandler
{
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

    public async Task Handle(Message message, CancellationToken ct)
    {
        // TODO logs here
        await ProcessUpdate(message, ct);
        // TODO logs here
    }

    protected abstract Task ProcessUpdate(Message message, CancellationToken ct);

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
