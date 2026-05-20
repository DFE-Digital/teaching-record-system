namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TpsEstablishment
{
    public const string LaCodeEstablishmentCodeIndexName = "ix_tps_establishments_la_code_establishment_number";

    public required Guid TpsEstablishmentId { get; set; }
    public required string LaCode { get; set; }
    public required string EstablishmentCode { get; set; }
    public required string EmployersName { get; set; }
    public required string? SchoolGiasName { get; set; }
    public required DateOnly? SchoolClosedDate { get; set; }
}
