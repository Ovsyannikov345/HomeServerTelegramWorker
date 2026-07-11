using Telegram.Bot.Types;

namespace HomeServerTelegramWorker.Telegram.CommandHandlers;

public interface ICommandHandler
{
    public string CommandName { get; }

    public string CommandDescription { get; }

    public string? CommandExample { get; }

    public bool CanHandle(Message message);

    public Task Handle(Message message, CancellationToken ct);
}
