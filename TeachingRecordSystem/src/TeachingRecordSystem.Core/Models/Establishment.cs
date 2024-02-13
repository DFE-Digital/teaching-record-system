namespace TeachingRecordSystem.Core.Models;

public class Establishment
{
    public required int Urn { get; init; }
    public required string LaCode { get; init; }
    public required string LaName { get; init; }
    public required string? EstablishmentNumber { get; init; }
    public required string EstablishmentName { get; init; }
    public required string EstablishmentTypeCode { get; init; }
    public required string EstablishmentTypeName { get; init; }
    public required int EstablishmentTypeGroupCode { get; init; }
    public required string EstablishmentTypeGroupName { get; init; }
    public required int EstablishmentStatusCode { get; init; }
    public required string EstablishmentStatusName { get; init; }
    public required string? Street { get; init; }
    public required string? Locality { get; init; }
    public required string? Address3 { get; init; }
    public required string? Town { get; init; }
    public required string? County { get; init; }
    public required string? Postcode { get; init; }
}
