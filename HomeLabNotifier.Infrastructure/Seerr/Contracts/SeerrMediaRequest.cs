using System.Text.Json.Serialization;

namespace HomeLabNotifier.Infrastructure.Seerr.Contracts;

internal sealed record SeerrMediaRequest
{
    [JsonPropertyName("mediaId")]
    public int MediaId { get; init; }

    [JsonPropertyName("mediaType")]
    public required string MediaType { get; init; }

    [JsonPropertyName("profileId")]
    public required int ProfileId { get; init; }

    [JsonPropertyName("serverId")]
    public required int ServerId { get; init; }

    [JsonPropertyName("seasons")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int>? Seasons { get; init; }
}
