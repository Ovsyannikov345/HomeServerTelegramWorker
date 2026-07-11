using HomeServerTelegramWorker.Seerr.Constants;
using System.Text.Json.Serialization;

namespace HomeServerTelegramWorker.Seerr.Dto;

public class SeerrSearchResponse
{
    [JsonPropertyName("results")]
    public List<SeerrMedia> Results { get; set; } = [];
}

public class SeerrMedia
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("mediaType")]
    public required string MediaType { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("overview")]
    public string Overview { get; set; } = string.Empty;

    [JsonPropertyName("releaseDate")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("firstAirDate")]
    public string? FirstAirDate { get; set; }

    [JsonPropertyName("posterPath")]
    public string? PosterPath { get; set; }

    public string DisplayTitle => Title ?? Name ?? "Unknown";

    public string DisplayType => MediaType is MediaTypes.Movie ? "Movie" : "Series";

    public string Year => (ReleaseDate ?? FirstAirDate ?? "    ").Length >= 4
        ? (ReleaseDate ?? FirstAirDate ?? "    ")[..4]
        : "N/A";
}
