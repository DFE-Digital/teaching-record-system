namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Country
{
    public required string CountryId { get; init; }
    public required string Name { get; set; }
    public required string OfficialName { get; set; }
    public required string CitizenNames { get; set; }
}
