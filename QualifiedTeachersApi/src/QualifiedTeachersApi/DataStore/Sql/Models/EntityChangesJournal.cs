namespace QualifiedTeachersApi.DataStore.Sql.Models;

public class EntityChangesJournal
{
    public required string Key { get; init; }
    public required string EntityLogicalName { get; init; }
    public required string DataToken { get; set; }
}
