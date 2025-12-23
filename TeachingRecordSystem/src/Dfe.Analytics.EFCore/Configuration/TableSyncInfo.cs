namespace Dfe.Analytics.EFCore.Configuration;

public record TableSyncInfo
{
    public required string Name { get; init; }
    public required TablePrimaryKeySyncInfo PrimaryKey { get; init; }
    public required IReadOnlyCollection<ColumnSyncInfo> Columns { get; init; }
}
