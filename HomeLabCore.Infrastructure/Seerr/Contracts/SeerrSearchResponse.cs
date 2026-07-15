using HomeLabCore.Infrastructure.Seerr.Constants;
using HomeLabCore.Infrastructure.Seerr.Constants.Enums;
using System.Text.Json.Serialization;

namespace HomeLabCore.Infrastructure.Seerr.Contracts;

internal sealed record SeerrSearchResponse
{
    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; init; }

    [JsonPropertyName("totalResults")]
    public int TotalResults { get; init; }

    [JsonPropertyName("results")]
    public List<SeerrMedia> Results { get; init; } = [];
}

internal sealed record SeerrMedia
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("mediaType")]
    public required string MediaType { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("overview")]
    public string Overview { get; init; } = string.Empty;

    [JsonPropertyName("releaseDate")]
    public string? ReleaseDate { get; init; }

    [JsonPropertyName("firstAirDate")]
    public string? FirstAirDate { get; init; }

    [JsonPropertyName("posterPath")]
    public string? PosterPath { get; init; }

    [JsonPropertyName("mediaInfo")]
    public SeerMediaMetadata? SeerMetadata { get; init; }
}

internal sealed record SeerMediaMetadata
{
    [JsonPropertyName("status")]
    public SeerrMediaStatus Status { get; init; } = SeerrMediaStatus.Unknown;
}
