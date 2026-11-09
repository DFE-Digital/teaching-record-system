using CsvHelper;

namespace TeachingRecordSystem.Core.Services.Establishments.Gias;

[CsvRecord]
public partial class EstablishmentCsvRow
{
    [CsvColumn("URN")]
    public required int Urn { get; init; }

    [CsvColumn("LA (code)")]
    public required string LaCode { get; init; }

    [CsvColumn("LA (name)")]
    public required string LaName { get; init; }

    [CsvColumn("EstablishmentNumber")]
    [CsvNullValues("")]
    public string? EstablishmentNumber { get; init; }

    [CsvColumn("EstablishmentName")]
    public required string EstablishmentName { get; init; }

    [CsvColumn("TypeOfEstablishment (code)")]
    public required string EstablishmentTypeCode { get; init; }

    [CsvColumn("TypeOfEstablishment (name)")]
    public required string EstablishmentTypeName { get; init; }

    [CsvColumn("EstablishmentTypeGroup (code)")]
    public required string EstablishmentGroupTypeCode { get; init; }

    [CsvColumn("EstablishmentTypeGroup (name)")]
    public required string EstablishmentGroupTypeName { get; init; }

    [CsvColumn("EstablishmentStatus (code)")]
    public required string EstablishmentStatusCode { get; init; }

    [CsvColumn("EstablishmentStatus (name)")]
    public required string EstablishmentStatusName { get; init; }

    [CsvColumn("PhaseOfEducation (code)")]
    public string? PhaseOfEducationCode { get; init; }

    [CsvColumn("PhaseOfEducation (name)")]
    public string? PhaseOfEducationName { get; init; }

    [CsvColumn("NumberOfPupils")]
    [CsvNullValues("")]
    public string? NumberOfPupils { get; init; }

    [CsvColumn("PercentageFSM")]
    [CsvNullValues("")]
    public string? FreeSchoolMealsPercentage { get; init; }

    [CsvColumn("Street")]
    [CsvNullValues("")]
    public string? Street { get; init; }

    [CsvColumn("Locality")]
    [CsvNullValues("")]
    public string? Locality { get; init; }

    [CsvColumn("Address3")]
    [CsvNullValues("")]
    public string? Address3 { get; init; }

    [CsvColumn("Town")]
    [CsvNullValues("")]
    public string? Town { get; init; }

    [CsvColumn("County (name)")]
    [CsvNullValues("")]
    public string? County { get; init; }

    [CsvColumn("Postcode")]
    [CsvNullValues("")]
    public string? Postcode { get; init; }


}
