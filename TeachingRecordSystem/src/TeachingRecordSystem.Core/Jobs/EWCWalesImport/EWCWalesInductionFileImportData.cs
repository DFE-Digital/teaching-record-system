using CsvHelper.Configuration.Attributes;

namespace TeachingRecordSystem.Core.Jobs.EWCWalesImport;

public class EWCWalesInductionFileImportData
{
    [Name("REFERENCE_NO")]
    public required string ReferenceNumber { get; init; }

    [Name("FIRST_NAME")]
    public required string FirstName { get; init; }

    [Name("LAST_NAME")]
    public required string LastName { get; init; }

    [Name("DATE_OF_BIRTH")]
    public required string DateOfBirth { get; init; }

    [Name("START_DATE")]
    public required string StartDate { get; init; }

    [Name("PASS_DATE")]
    public required string PassDate { get; init; }

    [Name("FAIL_DATE")]
    public required string FailDate { get; init; }

    [Name("EMPLOYER_NAME")]
    public required string EmployerName { get; init; }

    [Name("EMPLOYER_CODE")]
    public required string EmployerCode { get; init; }

    [Name("IND_STATUS_NAME")]
    public required string InductionStatusName { get; init; }
}
