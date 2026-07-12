using HomeLabCore.Application.Interfaces.Clients;
using HomeLabCore.Application.Interfaces.Database;
using HomeLabCore.Application.Telegram.Configuration;
using HomeLabCore.Application.Telegram.Exceptions;
using HomeLabCore.Application.Telegram.Services;
using HomeLabCore.Domain.Entities.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HomeLabCore.Application.Telegram.CommandHandlers;

internal sealed class SearchCommandHandler(
    IApplicationDbContext dbContext,
    ITelegramBotClient telegramBotClient,
    IMediaManagerClient mediaManagerClient,
    IMessageRenderer messageRenderer,
    IOptionsSnapshot<TelegramSettings> options,
    ILogger<SearchCommandHandler> logger)
    : CommandHandlerBase(telegramBotClient, options)
{
    private const int SearchResultsTotalCount = 20;

    public override bool RequiresAuthorization => true;

    public override string CommandName => "search";

    public override string CommandDescription => "Searches the requested movie or series";

    public override string? CommandExample => $"/{CommandName} The Matrix";

    protected override async Task ProcessUpdate(Message message, Message botResponseMessage, CancellationToken ct)
    {
        // TODO handle already downloaded
        // TODO handle season selection + already downloaded seasons

        var searchTerm = GetCommandArgument(message);

        if (searchTerm is null)
        {
            throw new CommandProcessingException($"Please provide a movie name. Example: `{CommandExample}`", showToUser: true);
        }

        await Task.Delay(2000, ct);

        searchTerm = searchTerm.Trim();

        // TODO proper logging
        logger.LogInformation("Chat {ChatId} searching for: {Query}", message.Chat.Id, searchTerm);

        var searchResults = await mediaManagerClient.Search(searchTerm, SearchResultsTotalCount, ct);

        if (searchResults.Count == 0)
        {
            throw new CommandProcessingException($"No results found for \"{searchTerm}\"", showToUser: true);
        }

        var searchSnapshot = new MediaSearchSnapshot
        {
            Query = searchTerm,
            Results = [.. searchResults.Select(m => new MediaSearchSnapshotEntry
            {
                Id = m.Id,
                MediaType = m.MediaType,
                Title = m.Title,
                Overview = m.Overview,
                ReleaseDate = m.ReleaseDate,
                FirstAirDate = m.FirstAirDate,
                PosterPath = m.PosterPath
            })]
        };

        dbContext.Add(searchSnapshot);
        await dbContext.SaveChanges(ct);

        await messageRenderer.SendMediaPage(
                chatId: message.Chat.Id,
                media: searchResults[0],
                searchId: searchSnapshot.Id,
                currentIndex: 0,
                hasNext: searchResults.Count > 1,
                ct: ct);

        await BotClient.DeleteMessage(
            chatId: botResponseMessage.Chat.Id,
            messageId: botResponseMessage.MessageId,
            cancellationToken: ct);
    }
}
