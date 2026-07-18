namespace HomeLabCore.Application.Telegram.CommandHandlers;

public sealed record CommandHandlerOptions
{
    public required bool RequiresAuthorization { get; init; }

    public required string CommandName { get; init; }

    public required string CommandDescription { get; init; }

    public required string? CommandExample { get; init; }
}
