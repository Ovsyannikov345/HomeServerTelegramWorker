using HomeLabCore.Domain.Constants.Enums;

namespace HomeLabCore.Infrastructure.Seerr.Constants.Enums;

internal enum SeerrMediaStatus
{
    Unknown = 1,
    Pending = 2,
    Processing = 3,
    PartiallyAvailable = 4,
    Available = 5,
    Deleted = 6
}

internal static class SeerrMediaStatusExtensions
{
    public static MediaStatus MapToInternalStatus(this SeerrMediaStatus status) => status switch
    {
        SeerrMediaStatus.Unknown => MediaStatus.Unknown,
        SeerrMediaStatus.Pending => MediaStatus.Pending,
        SeerrMediaStatus.Processing => MediaStatus.Processing,
        SeerrMediaStatus.PartiallyAvailable => MediaStatus.PartiallyAvailable,
        SeerrMediaStatus.Available => MediaStatus.Available,
        SeerrMediaStatus.Deleted => MediaStatus.Deleted,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown Seerr media status")
    };
}
