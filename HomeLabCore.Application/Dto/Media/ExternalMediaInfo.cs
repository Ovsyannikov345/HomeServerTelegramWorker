using HomeLabCore.Domain.Constants.Enums;
using HomeLabCore.Domain.Entities.Media;

namespace HomeLabCore.Application.Dto.Media;

public sealed record ExternalMediaInfo
{
    public int Id { get; init; }

    public required MediaType MediaType { get; init; }

    public required string Title { get; init; }

    public required string Overview { get; init; }

    public required MediaStatus Status { get; init; }

    public string? ReleaseDate { get; init; }

    public string? FirstAirDate { get; init; }

    public string? PosterPath { get; init; }

    public static ExternalMediaInfo FromSnapshot(MediaSearchSnapshotEntry snapshot)
    {
        return new ExternalMediaInfo
        {
            Id = snapshot.Id,
            MediaType = snapshot.MediaType,
            Title = snapshot.Title,
            Overview = snapshot.Overview,
            Status = snapshot.Status,
            ReleaseDate = snapshot.ReleaseDate,
            FirstAirDate = snapshot.FirstAirDate,
            PosterPath = snapshot.PosterPath
        };
    }
}
