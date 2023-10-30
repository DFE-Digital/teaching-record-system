namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class EntityChangesJournal
{
    public required string Key { get; init; }
    public required string EntityLogicalName { get; init; }
    public required string? DataToken { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string? LastUpdatedBy { get; set; }
    public int? NextQueryPageNumber { get; set; }
    public int? NextQueryPageSize { get; set; }
    public string? NextQueryPagingCookie { get; set; }
}
