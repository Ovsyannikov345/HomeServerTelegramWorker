using HomeLabCore.Application.Dto.Media;
using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Payloads;
using HomeLabCore.Application.Telegram.Constants;
using HomeLabCore.Domain.Constants.Enums;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HomeLabCore.Application.Telegram.Services;

public interface IMessageRenderer
{
    public Task SendMediaPage(
        long chatId,
        ExternalMediaInfo media,
        Guid searchId,
        int currentIndex,
        bool hasNext,
        CancellationToken ct);
}

public class MessageRenderer(ITelegramBotClient botClient) : IMessageRenderer
{
    public async Task SendMediaPage(
        long chatId,
        ExternalMediaInfo media,
        Guid searchId,
        int currentIndex,
        bool hasNext,
        CancellationToken ct)
    {
        var caption = BuildCaption();

        var keyboard = BuildKeyboard();

        if (!string.IsNullOrWhiteSpace(media.PosterPath))
        {
            await botClient.SendPhoto(
                chatId: chatId,
                photo: InputFile.FromUri(media.PosterPath),
                caption: caption,
                parseMode: ParseMode.Html,
                replyMarkup: keyboard,
                cancellationToken: ct);
        }
        else
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: caption,
                parseMode: ParseMode.Html,
                replyMarkup: keyboard,
                cancellationToken: ct);
        }

        string BuildCaption()
        {
            var releaseDate = media.ReleaseDate ?? media.FirstAirDate;

            if (releaseDate is not null)
            {
                releaseDate = $" ({releaseDate[..4]})";
            }

            var caption = new StringBuilder();
            caption.AppendLine($"🎬 <b>{media.Title}{releaseDate}</b>");
            caption.AppendLine($"<i>{media.MediaType.ToString().ToUpper()}</i>\n");
            caption.AppendLine(media.Overview.Length > 800 ? media.Overview[..800] + "..." : media.Overview);

            return caption.ToString();
        }

        InlineKeyboardMarkup BuildKeyboard()
        {
            var row1 = new InlineKeyboardButton[1];

            if (media.Status is MediaStatus.Available)
            {
                row1[0] = InlineKeyboardButton.WithCallbackData(
                    "✅ Available", 
                    new EmptyPayload().ToCallbackQueryString());
            }
            else
            {
                row1[0] = InlineKeyboardButton.WithCallbackData(
                    "⬇️ Download",
                    new RequestMediaPayload(media.MediaType, media.Id).ToCallbackQueryString());
            }

            var row2 = new List<InlineKeyboardButton>();

            if (currentIndex > 0)
            {
                row2.Add(InlineKeyboardButton.WithCallbackData(
                    "⬅️ Previous",
                    new ChangeSearchPagePayload(searchId, currentIndex - 1).ToCallbackQueryString()));
            }

            if (hasNext)
            {
                row2.Add(InlineKeyboardButton.WithCallbackData(
                    "➡️ Next",
                    new ChangeSearchPagePayload(searchId, currentIndex + 1).ToCallbackQueryString()));
            }

            var keyboardRows = new List<IEnumerable<InlineKeyboardButton>> { row1 };

            if (row2.Count > 0)
            {
                keyboardRows.Add(row2);
            }

            return new InlineKeyboardMarkup(keyboardRows);
        }
    }
}
