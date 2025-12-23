namespace Dfe.Analytics.EFCore.Configuration;

public record DatabaseSyncConfiguration
{
    public required IReadOnlyCollection<TableSyncInfo> Tables { get; init; }
}
