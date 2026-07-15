namespace HomeLabCore.Application.Telegram.Constants;

public static class CallbackQueryConstants
{
    public const string Delimiter = ":";

    public static class Prefixes
    {
        public const string RequestMedia = "REQ_MEDIA";

        public const string ChangePage = "CHN_PAGE";

        public const string Empty = "EMPT";
    }
}
