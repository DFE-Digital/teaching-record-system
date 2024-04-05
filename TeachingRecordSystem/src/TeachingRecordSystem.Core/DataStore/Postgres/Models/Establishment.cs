namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Establishment
{
    public const string UrnIndexName = "ix_establishment_urn";
    public const string LaCodeEstablishmentNumberIndexName = "ix_establishment_la_code_establishment_number";
    public const string EstablishmentSourceIdIndexName = "ix_establishment_establishment_source_id";

    public required Guid EstablishmentId { get; init; }
    public required int EstablishmentSourceId { get; set; }
    public required int? Urn { get; init; }
    public required string LaCode { get; set; }
    public required string? LaName { get; set; }
    public required string? EstablishmentNumber { get; set; }
    public required string EstablishmentName { get; set; }
    public required string? EstablishmentTypeCode { get; set; }
    public required string? EstablishmentTypeName { get; set; }
    public required int? EstablishmentTypeGroupCode { get; set; }
    public required string? EstablishmentTypeGroupName { get; set; }
    public required int? EstablishmentStatusCode { get; set; }
    public required string? EstablishmentStatusName { get; set; }
    public required string? Street { get; set; }
    public required string? Locality { get; set; }
    public required string? Address3 { get; set; }
    public required string? Town { get; set; }
    public required string? County { get; set; }
    public required string? Postcode { get; set; }
}
