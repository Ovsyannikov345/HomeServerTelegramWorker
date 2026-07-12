using System.Text.Json.Serialization;

namespace HomeLabNotifier.Infrastructure.Seerr.Contracts;

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
}
