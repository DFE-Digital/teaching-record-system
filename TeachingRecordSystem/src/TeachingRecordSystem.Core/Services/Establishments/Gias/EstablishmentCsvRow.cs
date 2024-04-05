using CsvHelper.Configuration.Attributes;

namespace TeachingRecordSystem.Core.Services.Establishments.Gias;

public class EstablishmentCsvRow
{
    [Name("URN")]
    public required int Urn { get; init; }

    [Name("LA (code)")]
    public required string LaCode { get; init; }

    [Name("LA (name)")]
    public required string LaName { get; init; }

    [Name("EstablishmentNumber")]
    [NullValues("")]
    public string? EstablishmentNumber { get; init; }

    [Name("EstablishmentName")]
    public required string EstablishmentName { get; init; }

    [Name("TypeOfEstablishment (code)")]
    public required string EstablishmentTypeCode { get; init; }

    [Name("TypeOfEstablishment (name)")]
    public required string EstablishmentTypeName { get; init; }

    [Name("EstablishmentTypeGroup (code)")]
    public required string EstablishmentGroupTypeCode { get; init; }

    [Name("EstablishmentTypeGroup (name)")]
    public required string EstablishmentGroupTypeName { get; init; }

    [Name("EstablishmentStatus (code)")]
    public required string EstablishmentStatusCode { get; init; }

    [Name("EstablishmentStatus (name)")]
    public required string EstablishmentStatusName { get; init; }

    [Name("Street")]
    [NullValues("")]
    public string? Street { get; init; }

    [Name("Locality")]
    [NullValues("")]
    public string? Locality { get; init; }

    [Name("Address3")]
    [NullValues("")]
    public string? Address3 { get; init; }

    [Name("Town")]
    [NullValues("")]
    public string? Town { get; init; }

    [Name("County (name)")]
    [NullValues("")]
    public string? County { get; init; }

    [Name("Postcode")]
    [NullValues("")]
    public string? Postcode { get; init; }
}
