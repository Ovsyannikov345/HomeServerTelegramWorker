using HomeLabCore.Domain.Constants.Enums;

namespace HomeLabCore.Domain.Entities.Media;

public sealed class MediaSearchSnapshot : EntityBase
{
    public required string Query { get; set; }

    public required List<MediaSearchSnapshotEntry> Results { get; set; }
}

public sealed record MediaSearchSnapshotEntry
{
    public int Id { get; init; }

    public required MediaType MediaType { get; init; }

    public required string Title { get; init; }

    public required string Overview { get; init; }

    public required MediaStatus Status { get; init; }

    public string? ReleaseDate { get; init; }

    public string? FirstAirDate { get; init; }

    public string? PosterPath { get; init; }
}
