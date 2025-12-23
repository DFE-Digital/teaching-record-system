namespace Dfe.Analytics.EFCore.Configuration;

public record TablePrimaryKeySyncInfo
{
    public required IReadOnlyCollection<string> ColumnNames { get; init; }
}
