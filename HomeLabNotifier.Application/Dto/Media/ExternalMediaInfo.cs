using HomeLabNotifier.Domain.Constants.Enums;

namespace HomeLabNotifier.Application.Dto.Media;

public sealed record ExternalMediaInfo
{
    public int Id { get; init; }

    public required MediaType MediaType { get; init; }

    public required string Title { get; init; }

    public required string Overview { get; init; }

    public string? ReleaseDate { get; init; }

    public string? FirstAirDate { get; init; }

    public string? PosterPath { get; init; }
}
