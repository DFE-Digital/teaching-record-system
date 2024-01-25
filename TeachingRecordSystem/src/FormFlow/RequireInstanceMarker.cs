namespace FormFlow;

internal sealed class RequireInstanceMarker
{
    public RequireInstanceMarker(int? errorStatusCode)
    {
        ErrorStatusCode = errorStatusCode;
    }

    public int? ErrorStatusCode { get; }
}
