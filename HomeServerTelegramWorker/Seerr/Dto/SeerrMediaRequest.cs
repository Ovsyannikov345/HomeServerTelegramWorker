using System.Text.Json.Serialization;

namespace HomeServerTelegramWorker.Seerr.Dto;

public sealed record SeerrMediaRequest
{
    [JsonPropertyName("mediaId")]
    public int MediaId { get; set; }

    [JsonPropertyName("mediaType")]
    public required string MediaType { get; set; }

    [JsonPropertyName("seasons")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int>? Seasons { get; set; }
}
