namespace TeachingRecordSystem.WebCommon.Infrastructure;

internal sealed class DbTransactionCreatedMarker
{
    private DbTransactionCreatedMarker()
    {
    }

    public static DbTransactionCreatedMarker Instance { get; } = new();
}
