using HomeLabCore.Application.Dto.Media;
using HomeLabCore.Application.Interfaces.Clients;
using HomeLabCore.Application.Interfaces.Database;
using HomeLabCore.Application.Logging;
using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Abstractions;
using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Payloads;
using HomeLabCore.Application.Telegram.Configuration;
using HomeLabCore.Application.Telegram.Constants;
using HomeLabCore.Application.Telegram.Exceptions;
using HomeLabCore.Application.Telegram.Services;
using HomeLabCore.Domain.Constants.Enums;
using HomeLabCore.Domain.Entities.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace HomeLabCore.Application.Telegram.CallbackQueryHandlers;

internal sealed class ChangeSearchPageQueryHandler(
    IApplicationDbContext dbContext,
    ITelegramBotClient telegramBotClient,
    IMediaManagerClient mediaManagerClient,
    IMessageRenderer messageRenderer,
    IOptionsSnapshot<TelegramSettings> options,
    ILogger<ChangeSearchPageQueryHandler> logger)
    : CallbackQueryHandlerBase<ChangeSearchPagePayload>(telegramBotClient, options, logger)
{
    protected override string QueryPrefix => CallbackQueryConstants.Prefixes.ChangePage;

    protected override bool RequiresAuthorization => true;

    protected override async Task ProcessCallbackQuery(CallbackQueryContext context, ChangeSearchPagePayload payload, CancellationToken ct)
    {
        var searchSnapshot = await dbContext
            .Query<MediaSearchSnapshot>()
            .FirstOrDefaultAsync(s => s.Id == payload.SearchId, ct);

        if (searchSnapshot is null || searchSnapshot.Results.Count == 0)
        {
            throw new CallbackQueryProcessingException("This search session has expired. Please search again.", showToUser: true);
        }

        if (payload.NextIndex < 0 || payload.NextIndex >= searchSnapshot.Results.Count)
        {
            throw new CallbackQueryProcessingException("Invalid index in payload.", showToUser: false);
        }

        var snapshotEntry = searchSnapshot.Results[payload.NextIndex];

        var mediaInfo = ExternalMediaInfo.FromSnapshot(snapshotEntry);

        // If the user requested a media, snapshot status becomes irrelevant
        try
        {
            // TODO handle series
            if (mediaInfo.MediaType is MediaType.Movie)
            {
                mediaInfo = mediaInfo with
                {
                    Status = await mediaManagerClient.GetMediaStatus(snapshotEntry.MediaType, snapshotEntry.Id, ct)
                };
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.FailedToFetchLatestMediaStatus(mediaInfo.MediaType, mediaInfo.Id);
        }

        var hasNext = payload.NextIndex < searchSnapshot.Results.Count - 1;

        var mediaPage = messageRenderer.RenderMediaSearchPage(
            media: mediaInfo,
            searchId: payload.SearchId,
            currentIndex: payload.NextIndex,
            hasNext: hasNext);

        await RespondWithMessage(mediaPage, ct);
        await DeleteOriginalMessage(CancellationToken.None);
    }
}
