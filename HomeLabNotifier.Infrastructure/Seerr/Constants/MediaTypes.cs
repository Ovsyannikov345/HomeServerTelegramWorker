using HomeLabNotifier.Domain.Constants.Enums;

namespace HomeLabNotifier.Infrastructure.Seerr.Constants;

internal static class MediaTypes
{
    public const string Movie = "movie";

    public const string Series = "tv";

    public const string Person = "person";

    public static MediaType ConvertToDomain(string seerMediaType) => seerMediaType switch
    {
        Movie => MediaType.Movie,
        Series => MediaType.Series,
        _ => throw new ArgumentOutOfRangeException(nameof(seerMediaType), seerMediaType, "Seer media type is not convertible to domain")
    };
}
