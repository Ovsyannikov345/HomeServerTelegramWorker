namespace HomeLabCore.Application.Telegram.Exceptions;

public sealed class CommandProcessingException : Exception
{
    public bool ShowMessageToUser { get; }

    public CommandProcessingException(string message, bool showToUser = false) : base(message, null)
    {
        ShowMessageToUser = showToUser;
    }
}
