using System.ComponentModel.DataAnnotations;

namespace HomeLabCore.Application.Telegram.Configuration;

internal sealed record TelegramSettings
{
    public const string SectionName = "Telegram";

    [Required]
    public required long[] UserIdWhitelist { get; init; }
}
