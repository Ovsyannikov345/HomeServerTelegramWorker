using HomeLabNotifier.Application.Dto.Media;
using HomeLabNotifier.Domain.Constants.Enums;

namespace HomeLabNotifier.Application.Interfaces.Clients;

public interface IMediaManagerClient
{
    public Task<List<ExternalMediaInfo>> Search(string query, int resultsCount, CancellationToken ct);

    public Task<bool> RequestMedia(MediaType mediaType, int mediaId, CancellationToken ct);
}
