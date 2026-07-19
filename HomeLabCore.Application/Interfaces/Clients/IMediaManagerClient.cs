using HomeLabCore.Application.Dto.Media;
using HomeLabCore.Domain.Constants.Enums;

namespace HomeLabCore.Application.Interfaces.Clients;

public interface IMediaManagerClient
{
    public Task<List<ExternalMediaInfo>> Search(string query, int resultsCount, CancellationToken ct);

    public Task RequestMedia(MediaType mediaType, int mediaId, CancellationToken ct);

    public Task<MediaStatus> GetMediaStatus(MediaType mediaType, int mediaId, CancellationToken ct);
}
