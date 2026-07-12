using HomeLabCore.Application.Dto.Media;
using HomeLabCore.Application.Interfaces.Database;
using HomeLabCore.Application.Telegram.CallbackQueryHandlers.Payloads;
using HomeLabCore.Application.Telegram.Configuration;
using HomeLabCore.Application.Telegram.Constants;
using HomeLabCore.Application.Telegram.Exceptions;
using HomeLabCore.Application.Telegram.Services;
using HomeLabCore.Domain.Entities.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace HomeLabCore.Application.Telegram.CallbackQueryHandlers;

// TODO add logging
internal sealed class ChangeSearchPageQueryHandler(
    IApplicationDbContext dbContext,
    ITelegramBotClient telegramBotClient,
    IMessageRenderer messageRenderer,
    IOptionsSnapshot<TelegramSettings> options)
    : CallbackQueryHandlerBase<ChangeSearchPagePayload>(telegramBotClient, options), ICallbackQueryHandler
{
    protected override string QueryPrefix => CallbackQueryConstants.Prefixes.ChangePage;

    protected override bool RequiresAuthorization => true;

    protected override async Task ProcessCallbackQuery(CallbackQuery callbackQuery, ChangeSearchPagePayload payload, CancellationToken ct)
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

        var hasNext = payload.NextIndex < searchSnapshot.Results.Count - 1;

        await messageRenderer.SendMediaPage(
            chatId: callbackQuery.Message!.Chat.Id,
            media: ExternalMediaInfo.FromSnapshot(snapshotEntry),
            searchId: payload.SearchId,
            currentIndex: payload.NextIndex,
            hasNext: hasNext,
            ct: ct);

        await BotClient.DeleteMessage(
            chatId: callbackQuery.Message.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            cancellationToken: ct);
    }
}
