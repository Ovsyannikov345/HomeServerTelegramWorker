using HomeServerTelegramWorker.Seerr;
using HomeServerTelegramWorker.Telegram.CallbackQueryHandlers.Payloads;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HomeServerTelegramWorker.Telegram.CommandHandlers;

public sealed class SearchCommandHandler(
    ITelegramBotClient telegramBotClient,
    ISeerrClient seerrClient,
    ILogger<SearchCommandHandler> logger) 
    : CommandHandlerBase, ICommandHandler
{
    public override string CommandName => "search";

    public override string CommandDescription => "Searches the requested movie or series";

    public override string? CommandExample => $"/{CommandName} The Matrix";

    protected override async Task ProcessUpdate(Message message, CancellationToken ct)
    {
        // TODO handle already downloaded
        // TODO handle season selection + already downloaded seasons

        var searchTerm = GetCommandArgument(message);

        if (searchTerm is null)
        {
            await telegramBotClient.SendMessage(
                chatId: message.Chat.Id,
                text: $"Please provide a movie name. Example: `{CommandExample}`",
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);

            return;
        }

        searchTerm = searchTerm.Trim();

        // TODO proper logging
        logger.LogInformation("Chat {ChatId} searching for: {Query}", message.Chat.Id, searchTerm);

        var searchResults = await seerrClient.Search(searchTerm, ct);

        if (searchResults.Count == 0)
        {
            await telegramBotClient.SendMessage(
                chatId: message.Chat.Id,
                text: "No results found in Seerr.",
                cancellationToken: ct);
            return;
        }

        // TODO implement "Next" button and pagination
        var media = searchResults[0];

        var caption = new StringBuilder();
        caption.AppendLine($"🎬 <b>{media.DisplayTitle} ({media.Year})</b>");
        caption.AppendLine($"<i>{media.DisplayType.ToUpper()}</i>\n");
        caption.AppendLine(media.Overview.Length > 800 ? media.Overview[..800] + "..." : media.Overview);

        var downloadQueryPayload = new RequestMediaPayload
        {
            MediaId = media.Id,
            MediaType = media.MediaType
        };

        var buttons = new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData(
            text: "⬇️ Download",
            callbackData: downloadQueryPayload.ToCallbackQueryString())
        };

        // TODO implement "Next" button and pagination
        if (true)
        {
            buttons.Add(InlineKeyboardButton.WithCallbackData(
                text: "➡️ Next",
                callbackData: "dummy payload"));
        }

        var keyboard = new InlineKeyboardMarkup(buttons);

        if (!string.IsNullOrEmpty(media.PosterPath))
        {
            // TODO to config
            var imageUrl = $"https://image.tmdb.org/t/p/w500{media.PosterPath}";

            await telegramBotClient.SendPhoto(
                chatId: message.Chat.Id,
                photo: InputFile.FromUri(imageUrl),
                caption: caption.ToString(),
                parseMode: ParseMode.Html,
                replyMarkup: keyboard,
                cancellationToken: ct);
        }
        else
        {
            await telegramBotClient.SendMessage(
                chatId: message.Chat.Id,
                text: caption.ToString(),
                parseMode: ParseMode.Html,
                replyMarkup: keyboard,
                cancellationToken: ct);
        }
    }
}
