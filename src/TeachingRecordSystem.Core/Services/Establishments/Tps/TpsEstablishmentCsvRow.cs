using CsvHelper.Configuration.Attributes;

namespace TeachingRecordSystem.Core.Services.Establishments.Tps;

public class TpsEstablishmentCsvRow
{
    [Name("LA Code")]
    public required string LaCode { get; set; }
    [Name("Establishment Code")]
    public required string EstablishmentCode { get; set; }
    [Name("EMPS Name")]
    public required string EmployersName { get; set; }
    [Name("SCHL (GIAS) Name")]
    [NullValues("")]
    public required string? SchoolGiasName { get; set; }
    [Name("School Closed Date")]
    [NullValues("")]
    public required string? SchoolClosedDate { get; set; }
}
