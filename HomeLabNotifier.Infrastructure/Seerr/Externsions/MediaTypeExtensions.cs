using HomeLabNotifier.Domain.Constants.Enums;
using HomeLabNotifier.Infrastructure.Seerr.Constants;

namespace HomeLabNotifier.Infrastructure.Seerr.Externsions;

internal static class MediaTypeExtensions
{
    public static string ToSeerrMediaType(this MediaType mediaType) => mediaType switch
    {
        MediaType.Movie => MediaTypes.Movie,
        MediaType.Series => MediaTypes.Series,
        _ => throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, null)
    };
}
