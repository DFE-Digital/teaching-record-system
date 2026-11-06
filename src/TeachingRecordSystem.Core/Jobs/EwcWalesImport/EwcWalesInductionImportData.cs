using CsvHelper;

namespace TeachingRecordSystem.Core.Jobs.EwcWalesImport;

[CsvRecord]
public partial record EwcWalesInductionImportData
{
    [CsvColumn("REFERENCE_NO")]
    public required string ReferenceNumber { get; set; }

    [CsvColumn("FIRST_NAME")]
    public required string FirstName { get; set; }

    [CsvColumn("LAST_NAME")]
    public required string LastName { get; set; }

    [CsvColumn("DATE_OF_BIRTH")]
    public required string DateOfBirth { get; set; }

    [CsvColumn("START_DATE")]
    public required string StartDate { get; set; }

    [CsvColumn("PASS_DATE")]
    public required string PassedDate { get; set; }

    [CsvColumn("FAIL_DATE")]
    public required string FailDate { get; set; }

    [CsvColumn("EMPLOYER_NAME")]
    public required string EmployerName { get; set; }

    [CsvColumn("EMPLOYER_CODE")]
    public required string EmployerCode { get; set; }

    [CsvColumn("IND_STATUS_NAME")]
    public required string InductionStatusName { get; set; }
}


