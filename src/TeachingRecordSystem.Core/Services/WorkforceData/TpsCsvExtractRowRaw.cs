using CsvHelper.Configuration.Attributes;

namespace TeachingRecordSystem.Core.Services.WorkforceData;

public class TpsCsvExtractRowRaw
{
    [Name("Teachers Pensions Reference Number (TRN)")]
    [NullValues("")]
    public required string? Trn { get; init; }
    [Name("National Insurance Number (NINO)")]
    [NullValues("")]
    public required string? NationalInsuranceNumber { get; init; }
    [Name("Date of Birth (DOB)")]
    [NullValues("")]
    public required string? DateOfBirth { get; init; }
    [Name("Date of Death")]
    [NullValues("")]
    public required string? DateOfDeath { get; init; }
    [Name("Postcode")]
    [NullValues("")]
    public required string? MemberPostcode { get; init; }
    [Name("Email Address (Member)")]
    [NullValues("")]
    public required string? MemberEmailAddress { get; init; }
    [Name("Local Authority Code")]
    [NullValues("")]
    public required string? LocalAuthorityCode { get; init; }
    [Name("Establishment Code")]
    [NullValues("")]
    public required string? EstablishmentCode { get; init; }
    [Name("Postcode (Establishment)")]
    [NullValues("")]
    public required string? EstablishmentPostcode { get; init; }
    [Name("Email Address (Establishment)")]
    [NullValues("")]
    public required string? EstablishmentEmailAddress { get; init; }
    [Name("Start Date")]
    [NullValues("")]
    public required string? EmploymentStartDate { get; init; }
    [Name("End Date")]
    [NullValues("")]
    public required string? EmploymentEndDate { get; init; }
    [Name("Full Time / Part Time Indicator")]
    [NullValues("")]
    public required string? FullOrPartTimeIndicator { get; init; }
    [Name("Withdrawal Indicator")]
    [NullValues("")]
    public required string? WithdrawlIndicator { get; init; }
    [Name("Extract Date")]
    [NullValues("")]
    public required string? ExtractDate { get; init; }
    [Name("Gender")]
    [NullValues("")]
    public required string? Gender { get; init; }
}
