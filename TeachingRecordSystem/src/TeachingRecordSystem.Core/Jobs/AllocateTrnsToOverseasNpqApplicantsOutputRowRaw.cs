using CsvHelper.Configuration.Attributes;

namespace TeachingRecordSystem.Core.Jobs;

public class AllocateTrnsToOverseasNpqApplicantsOutputRowRaw
{
    [Name("First Name (Required)")]
    [NullValues("")]
    public string? FirstName { get; set; }

    [Name("Middle Name (Optional but preferable)")]
    [NullValues("")]
    public string? MiddleName { get; set; }

    [Name("Surname/Last Name (Required)")]
    [NullValues("")]
    public string? LastName { get; set; }

    [Name("Date of Birth (Required)")]
    [NullValues("")]
    public string? DateOfBirth { get; set; }

    [Name("PERSONAL Email Address (Required)")]
    [NullValues("")]
    public string? EmailAddress { get; set; }

    [Name("NI Number (Optional but preferable)")]
    [NullValues("")]
    public string? NationalInsuranceNumber { get; set; }

    [Name("Nationality (Optional but preferable)")]
    [NullValues("")]
    public string? Nationality { get; set; }

    [Name("Gender (Optional but preferable)")]
    [NullValues("")]
    public string? Gender { get; set; }

    [Name("Has the Participant started their NPQ or is confirmed to start in November 2025 (Y/N) Please Note -  This needs to be a yes for all applicants when this completed  list is returned (Required)")]
    [NullValues("")]
    public string? ConfirmedStartedOrDueToStartNpq { get; set; }

    [Name("Result")]
    [NullValues("")]
    public string? Result { get; set; }

    [Name("Errors")]
    [NullValues("")]
    public string? Errors { get; set; }

    [Name("Allocated TRN")]
    [NullValues("")]
    public string? AllocatedTrn { get; set; }

    [Name("Potential duplicate TRNs")]
    [NullValues("")]
    public string? PotentialDuplicateTrns { get; set; }
}
