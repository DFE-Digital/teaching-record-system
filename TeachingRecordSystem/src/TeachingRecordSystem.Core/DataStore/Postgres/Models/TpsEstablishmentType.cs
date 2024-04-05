namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TpsEstablishmentType
{
    public required int TpsEstablishmentTypeId { get; set; }
    public required string EstablishmentRangeFrom { get; set; }
    public required string EstablishmentRangeTo { get; set; }
    public required string Description { get; set; }
    public required string ShortDescription { get; set; }
}
