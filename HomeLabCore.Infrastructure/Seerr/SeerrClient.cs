using HomeLabCore.Application.Dto.Media;
using HomeLabCore.Application.Interfaces.Clients;
using HomeLabCore.Domain.Constants.Enums;
using HomeLabCore.Infrastructure.Seerr.Configuration;
using HomeLabCore.Infrastructure.Seerr.Constants;
using HomeLabCore.Infrastructure.Seerr.Constants.Enums;
using HomeLabCore.Infrastructure.Seerr.Contracts;
using HomeLabCore.Infrastructure.Seerr.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace HomeLabCore.Infrastructure.Seerr;

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
                        Status = m.SeerMetadata?.Status.MapToInternalStatus() ?? MediaStatus.Unknown,
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

    public async Task RequestMedia(MediaType mediaType, int mediaId, CancellationToken ct)
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

        response.EnsureSuccessStatusCode();
    }

    public async Task<MediaStatus> GetMediaStatus(MediaType mediaType, int mediaId, CancellationToken ct)
    {
        // TODO implement series
        if (mediaType is MediaType.Series)
        {
            throw new NotImplementedException("Series handling is not implemented yet");
        }

        var requestUrl = $"api/v1/movie/{mediaId}";

        var response = await httpClient.GetAsync(requestUrl, ct);

        response.EnsureSuccessStatusCode();

        var movieDetails = await response.Content.ReadFromJsonAsync<SeerMovieDetailsResponse>(ct);

        return movieDetails?.SeerMetadata?.Status.MapToInternalStatus() ?? MediaStatus.Unknown;
    }
}
