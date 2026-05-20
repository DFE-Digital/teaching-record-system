using CsvHelper.Configuration.Attributes;

namespace TeachingRecordSystem.Core.Jobs.EwcWalesImport;

public record EwcWalesInductionImportData
{
    [Name("REFERENCE_NO")]
    public required string ReferenceNumber { get; set; }

    [Name("FIRST_NAME")]
    public required string FirstName { get; set; }

    [Name("LAST_NAME")]
    public required string LastName { get; set; }

    [Name("DATE_OF_BIRTH")]
    public required string DateOfBirth { get; set; }

    [Name("START_DATE")]
    public required string StartDate { get; set; }

    [Name("PASS_DATE")]
    public required string PassedDate { get; set; }

    [Name("FAIL_DATE")]
    public required string FailDate { get; set; }

    [Name("EMPLOYER_NAME")]
    public required string EmployerName { get; set; }

    [Name("EMPLOYER_CODE")]
    public required string EmployerCode { get; set; }

    [Name("IND_STATUS_NAME")]
    public required string InductionStatusName { get; set; }
}


