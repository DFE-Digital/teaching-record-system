using CsvHelper;

namespace TeachingRecordSystem.Core.Services.WorkforceData;

[CsvRecord]
public partial class TpsCsvExtractRowRaw
{
    [CsvColumn("Teachers Pensions Reference Number (TRN)")]
    [CsvNullValues("")]
    public required string? Trn { get; init; }
    [CsvColumn("National Insurance Number (NINO)")]
    [CsvNullValues("")]
    public required string? NationalInsuranceNumber { get; init; }
    [CsvColumn("Date of Birth (DOB)")]
    [CsvNullValues("")]
    public required string? DateOfBirth { get; init; }
    [CsvColumn("Date of Death")]
    [CsvNullValues("")]
    public required string? DateOfDeath { get; init; }
    [CsvColumn("Postcode")]
    [CsvNullValues("")]
    public required string? MemberPostcode { get; init; }
    [CsvColumn("Email Address (Member)")]
    [CsvNullValues("")]
    public required string? MemberEmailAddress { get; init; }
    [CsvColumn("Local Authority Code")]
    [CsvNullValues("")]
    public required string? LocalAuthorityCode { get; init; }
    [CsvColumn("Establishment Code")]
    [CsvNullValues("")]
    public required string? EstablishmentCode { get; init; }
    [CsvColumn("Postcode (Establishment)")]
    [CsvNullValues("")]
    public required string? EstablishmentPostcode { get; init; }
    [CsvColumn("Email Address (Establishment)")]
    [CsvNullValues("")]
    public required string? EstablishmentEmailAddress { get; init; }
    [CsvColumn("Start Date")]
    [CsvNullValues("")]
    public required string? EmploymentStartDate { get; init; }
    [CsvColumn("End Date")]
    [CsvNullValues("")]
    public required string? EmploymentEndDate { get; init; }
    [CsvColumn("Full Time / Part Time Indicator")]
    [CsvNullValues("")]
    public required string? FullOrPartTimeIndicator { get; init; }
    [CsvColumn("Withdrawal Indicator")]
    [CsvNullValues("")]
    public required string? WithdrawlIndicator { get; init; }
    [CsvColumn("Extract Date")]
    [CsvNullValues("")]
    public required string? ExtractDate { get; init; }
    [CsvColumn("Gender")]
    [CsvNullValues("")]
    public required string? Gender { get; init; }
}
