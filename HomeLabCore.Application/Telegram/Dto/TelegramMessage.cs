using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace HomeLabCore.Application.Telegram.Dto;

internal sealed record TelegramMessage
{
    public required string Caption { get; init; }

    public InlineKeyboardMarkup? Keyboard { get; init; }

    public InputFile? Photo { get; init; }
}
