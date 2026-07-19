using HomeLabCore.Domain.Constants.Enums;
using HomeLabCore.Infrastructure.Seerr.Constants;

namespace HomeLabCore.Infrastructure.Seerr.Extensions;

internal static class MediaTypeExtensions
{
    public static string ToSeerrMediaType(this MediaType mediaType) => mediaType switch
    {
        MediaType.Movie => MediaTypes.Movie,
        MediaType.Series => MediaTypes.Series,
        _ => throw new ArgumentOutOfRangeException(nameof(mediaType), mediaType, null)
    };
}
