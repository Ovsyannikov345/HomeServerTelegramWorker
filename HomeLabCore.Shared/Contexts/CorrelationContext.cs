namespace HomeLabCore.Shared.Contexts;

public static class CorrelationContext
{
    private static readonly AsyncLocal<string?> _correlationId = new();

    public static string CorrelationId
    {
        get
        {
            _correlationId.Value ??= Guid.CreateVersion7().ToString("N");

            return _correlationId.Value;
        }
        set => _correlationId.Value = value;
    }
}
