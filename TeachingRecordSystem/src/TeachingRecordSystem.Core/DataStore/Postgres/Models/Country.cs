namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Country
{
    public required string CountryId { get; init; }
    public required string Name { get; set; }
}
