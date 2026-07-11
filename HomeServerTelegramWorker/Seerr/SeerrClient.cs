using HomeServerTelegramWorker.Seerr.Constants;
using HomeServerTelegramWorker.Seerr.Dto;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace HomeServerTelegramWorker.Seerr;

public interface ISeerrClient
{
    public Task<List<SeerrMedia>> Search(string query, CancellationToken ct);

    public Task<bool> RequestMedia(string mediaType, int mediaId, CancellationToken ct);
}

public sealed class SeerrClient(HttpClient httpClient, IOptions<SeerrSettings> options) : ISeerrClient
{
    private readonly SeerrSettings _settings = options.Value;

    public async Task<List<SeerrMedia>> Search(string query, CancellationToken ct)
    {
        var requestUrl = QueryHelpers.AddQueryString("api/v1/search", new Dictionary<string, string?>
        {
            ["query"] = query,
        });

        var response = await httpClient.GetAsync(requestUrl, ct);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SeerrSearchResponse>(cancellationToken: ct);

        return result?.Results.Where(m => m.MediaType is "movie" or "tv").ToList() ?? [];
    }

    public async Task<bool> RequestMedia(string mediaType, int mediaId, CancellationToken ct)
    {
        // TODO implement series
        if (mediaType is MediaTypes.Series)
        {
            throw new NotImplementedException("Series requesting is not implemented yet");
        }

        var requestUrl = $"api/v1/request";

        var payload = new SeerrMediaRequest
        {
            MediaId = mediaId,
            MediaType = mediaType,
            Seasons = null
        };

        var response = await httpClient.PostAsJsonAsync(requestUrl, payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);

            return false;
        }

        return true;
    }
}
