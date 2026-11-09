using CsvHelper;

namespace TeachingRecordSystem.Core.Services.Establishments.Tps;

[CsvRecord]
public partial class TpsEstablishmentCsvRow
{
    [CsvColumn("LA Code")]
    public required string LaCode { get; set; }
    [CsvColumn("Establishment Code")]
    public required string EstablishmentCode { get; set; }
    [CsvColumn("EMPS Name")]
    public required string EmployersName { get; set; }
    [CsvColumn("SCHL (GIAS) Name")]
    [CsvNullValues("")]
    public required string? SchoolGiasName { get; set; }
    [CsvColumn("School Closed Date")]
    [CsvNullValues("")]
    public required string? SchoolClosedDate { get; set; }
}
