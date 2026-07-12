using System.ComponentModel.DataAnnotations;

namespace HomeLabCore.Infrastructure.Telegram.Configuration;

internal sealed record TelegramSettings
{
    public const string SectionName = "Telegram";

    [Required(AllowEmptyStrings = false)]
    public required string BotToken { get; init; }
}
