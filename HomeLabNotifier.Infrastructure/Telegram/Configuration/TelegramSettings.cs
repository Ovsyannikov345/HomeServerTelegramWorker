using System.ComponentModel.DataAnnotations;

namespace HomeLabNotifier.Infrastructure.Telegram.Configuration;

internal sealed record TelegramSettings
{
    public const string SectionName = "Telegram";

    [Required(AllowEmptyStrings = false)]
    public required string BotToken { get; init; }

    public required long[] ChatIdWhitelist { get; init; }
}
