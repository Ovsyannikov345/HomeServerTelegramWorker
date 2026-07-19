using HomeLabCore.Application.Dto.Media;
using HomeLabCore.Application.Interfaces.Clients;
using HomeLabCore.Application.Interfaces.Database;
using HomeLabCore.Application.Telegram.CommandHandlers.Abstractions;
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
    : CommandHandlerBase(telegramBotClient, options, logger)
{
    private const int SearchResultsTotalCount = 20;

    public override CommandHandlerOptions HandlerOptions => new()
    {
        RequiresAuthorization = true,
        CommandName = "search",
        CommandDescription = "Searches the requested movie or series",
        CommandExample = $"/search The Matrix"
    };

    protected override async Task ProcessUpdate(Message message, Message botResponseMessage, CancellationToken ct)
    {
        var searchTerm = GetCommandArgument(message);

        if (searchTerm is null)
        {
            throw new CommandProcessingException($"Please provide a media name. Example: `{HandlerOptions.CommandExample}`", showToUser: true);
        }

        searchTerm = searchTerm.Trim();

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
                Status = m.Status,
                ReleaseDate = m.ReleaseDate,
                FirstAirDate = m.FirstAirDate,
                PosterPath = m.PosterPath
            })]
        };

        dbContext.Add(searchSnapshot);
        await dbContext.SaveChanges(ct);

        var mediaPage = messageRenderer.RenderMediaSearchPage(
            media: searchResults[0],
            searchId: searchSnapshot.Id,
            currentIndex: 0,
            hasNext: searchResults.Count > 1);

        await RespondWithMessage(mediaPage, ct);
    }
}
