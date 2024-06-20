namespace TeachingRecordSystem.UiCommon.FormFlow;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireJourneyInstanceAttribute : Attribute
{
    public RequireJourneyInstanceAttribute()
    {
    }

    public RequireJourneyInstanceAttribute(int errorStatusCode)
    {
        if (errorStatusCode < 400 || errorStatusCode > 599)
        {
            throw new ArgumentOutOfRangeException(nameof(errorStatusCode));
        }

        ErrorStatusCode = errorStatusCode;

    }
    public int? ErrorStatusCode { get; }
}
