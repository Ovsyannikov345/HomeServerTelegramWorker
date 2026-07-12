namespace HomeLabNotifier.Application.Telegram.Exceptions;

public sealed class CallbackQueryProcessingException(string message, Exception? innerException) : Exception(message, innerException)
{
    public CallbackQueryProcessingException(string message) : this(message, null)
    {
    }
}
