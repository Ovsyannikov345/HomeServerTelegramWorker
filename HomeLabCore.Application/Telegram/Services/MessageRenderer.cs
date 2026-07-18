using HomeLabCore.Application.Dto.Media;
using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Payloads;
using HomeLabCore.Application.Telegram.Dto;
using HomeLabCore.Domain.Constants.Enums;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace HomeLabCore.Application.Telegram.Services;

internal interface IMessageRenderer
{
    public TelegramMessage RenderMediaSearchPage(
        ExternalMediaInfo media,
        Guid searchId,
        int currentIndex,
        bool hasNext);
}

internal class MessageRenderer : IMessageRenderer
{
    private const string PlaceholderImagePath = "Assets/Images/no_image_placeholder.jpg";

    public TelegramMessage RenderMediaSearchPage(
        ExternalMediaInfo media,
        Guid searchId,
        int currentIndex,
        bool hasNext)
    {
        var caption = BuildCaption();

        var keyboard = BuildKeyboard();

        InputFile photo;

        if (!string.IsNullOrWhiteSpace(media.PosterPath))
        {
            photo = InputFile.FromUri(media.PosterPath);
        }
        else
        {
            var fullPlaceholderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PlaceholderImagePath);
            var fileStream = File.OpenRead(fullPlaceholderPath);

            photo = InputFile.FromStream(fileStream, Path.GetFileName(fullPlaceholderPath));
        }

        return new TelegramMessage()
        {
            Caption = caption,
            Keyboard = keyboard,
            Photo = photo
        };

        string BuildCaption()
        {
            var releaseDate = media.ReleaseDate ?? media.FirstAirDate;

            if (releaseDate is not null)
            {
                releaseDate = $" ({releaseDate[..4]})";
            }

            var caption = new StringBuilder();
            caption.AppendLine($"🎬 **{media.Title}{releaseDate}**");
            caption.AppendLine($"*{media.MediaType.ToString().ToUpper()}*\n");
            caption.AppendLine(media.Overview.Length > 800 ? media.Overview[..800] + "..." : media.Overview);

            return caption.ToString();
        }

        InlineKeyboardMarkup BuildKeyboard()
        {
            var row1 = new InlineKeyboardButton[1];

            // TODO this does not support series (Partially available status)
            if (media.Status is MediaStatus.Available)
            {
                row1[0] = InlineKeyboardButton.WithCallbackData(
                    "✅ Available",
                    new EmptyPayload().ToCallbackQueryString());
            }
            if (media.Status is MediaStatus.Pending or MediaStatus.Processing)
            {
                row1[0] = InlineKeyboardButton.WithCallbackData(
                    "⏳ Processing...",
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
