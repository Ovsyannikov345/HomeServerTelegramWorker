using HomeLabNotifier.Application.Interfaces.Clients;
using HomeLabNotifier.Application.Telegram.CallbackQueryHandlers.Payloads;
using Microsoft.Extensions.Logging;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HomeLabNotifier.Application.Telegram.CommandHandlers;

public sealed class SearchCommandHandler(
    ITelegramBotClient telegramBotClient,
    IMediaManagerClient mediaManagerClient,
    ILogger<SearchCommandHandler> logger) 
    : CommandHandlerBase, ICommandHandler
{
    private const int SearchResultsTotalCount = 20;

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

        var searchResults = await mediaManagerClient.Search(searchTerm, SearchResultsTotalCount, ct);

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

        var releaseDate = media.ReleaseDate ?? media.FirstAirDate;

        if (releaseDate is not null)
        {
            releaseDate = $" ({releaseDate[..4]})";
        }

        var caption = new StringBuilder();
        caption.AppendLine($"🎬 <b>{media.Title}{releaseDate}</b>");
        caption.AppendLine($"<i>{media.MediaType.ToString().ToUpper()}</i>\n");
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
            await telegramBotClient.SendPhoto(
                chatId: message.Chat.Id,
                photo: InputFile.FromUri(media.PosterPath),
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
