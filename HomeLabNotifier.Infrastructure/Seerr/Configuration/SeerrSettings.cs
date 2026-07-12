using System.ComponentModel.DataAnnotations;

namespace HomeLabNotifier.Infrastructure.Seerr.Configuration;

internal sealed record SeerrSettings
{
    public const string SectionName = "Seerr";

    [Required(AllowEmptyStrings = false)]
    public required string BaseUrl { get; init; }

    [Required(AllowEmptyStrings = false)]
    public required string ApiKey { get; init; }

    [Url]
    public required string MediaPosterUrlBase { get; init; }

    [Required]
    [Range(0, int.MaxValue)]
    public int RadarrAndSonarrId { get; init; }

    [Required]
    [Range(0, int.MaxValue)]
    public int QualityProfileId { get; init; }
}
