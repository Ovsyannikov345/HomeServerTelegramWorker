using HomeLabCore.Infrastructure.Seerr.Constants.Enums;
using System.Text.Json.Serialization;

namespace HomeLabCore.Infrastructure.Seerr.Contracts;

internal sealed record SeerMovieDetailsResponse
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("mediaInfo")]
    public SeerMovieMetadata? SeerMetadata { get; init; }
}

internal sealed record SeerMovieMetadata
{
    [JsonPropertyName("status")]
    public SeerrMediaStatus Status { get; init; } = SeerrMediaStatus.Unknown;
}
