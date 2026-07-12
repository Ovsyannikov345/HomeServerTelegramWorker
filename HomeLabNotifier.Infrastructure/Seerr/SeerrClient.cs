using HomeLabNotifier.Application.Dto.Media;
using HomeLabNotifier.Application.Interfaces.Clients;
using HomeLabNotifier.Domain.Constants.Enums;
using HomeLabNotifier.Infrastructure.Seerr.Configuration;
using HomeLabNotifier.Infrastructure.Seerr.Constants;
using HomeLabNotifier.Infrastructure.Seerr.Contracts;
using HomeLabNotifier.Infrastructure.Seerr.Externsions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace HomeLabNotifier.Infrastructure.Seerr;

internal sealed class SeerrClient(HttpClient httpClient, IOptionsMonitor<SeerrSettings> options) : IMediaManagerClient
{
    public async Task<List<ExternalMediaInfo>> Search(string query, int resultsCount, CancellationToken ct)
    {
        var settings = options.CurrentValue;

        List<ExternalMediaInfo> totalResults = [];

        int currentPage = 1;

        while (totalResults.Count < resultsCount)
        {
            var requestUrl = QueryHelpers.AddQueryString("api/v1/search", new Dictionary<string, string?>
            {
                ["query"] = query,
                ["page"] = currentPage.ToString()
            });

            var response = await httpClient.GetAsync(requestUrl, ct);

            response.EnsureSuccessStatusCode();

            var mediaSearchResult = await response.Content.ReadFromJsonAsync<SeerrSearchResponse>(cancellationToken: ct);

            if (mediaSearchResult?.Results is null || mediaSearchResult.Results.Count == 0)
            {
                break;
            }

            totalResults.AddRange(
                mediaSearchResult.Results
                    .Where(m => m.MediaType != MediaTypes.Person)
                    .Select(m => new ExternalMediaInfo
                    {
                        Id = m.Id,
                        MediaType = MediaTypes.ConvertToDomain(m.MediaType),
                        Title = m.Title ?? m.Name ?? "Unknown",
                        Overview = m.Overview,
                        ReleaseDate = m.ReleaseDate,
                        FirstAirDate = m.FirstAirDate,
                        PosterPath = string.IsNullOrWhiteSpace(m.PosterPath)
                            ? null
                            : $"{settings.MediaPosterUrlBase.TrimEnd('/')}/{m.PosterPath.TrimStart('/')}"
                    }));

            if (mediaSearchResult.TotalPages <= currentPage)
            {
                break;
            }

            currentPage++;
        }

        return [.. totalResults.Take(resultsCount)];
    }

    public async Task<bool> RequestMedia(MediaType mediaType, int mediaId, CancellationToken ct)
    {
        var settings = options.CurrentValue;

        // TODO implement series
        if (mediaType is MediaType.Series)
        {
            throw new NotImplementedException("Series requesting is not implemented yet");
        }

        var requestUrl = $"api/v1/request";

        var payload = new SeerrMediaRequest
        {
            MediaId = mediaId,
            MediaType = mediaType.ToSeerrMediaType(),
            ProfileId = settings.QualityProfileId,
            ServerId = settings.RadarrAndSonarrId,
            Seasons = null
        };

        var response = await httpClient.PostAsJsonAsync(requestUrl, payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        return true;
    }
}
