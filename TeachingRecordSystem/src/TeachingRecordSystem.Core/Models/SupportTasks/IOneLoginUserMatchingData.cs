using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Models.SupportTasks;

public interface IOneLoginUserMatchingData : ISupportTaskData
{
    string OneLoginUserSubject { get; init; }
    string? StatedNationalInsuranceNumber { get; init; }
    string? StatedTrn { get; init; }
    string? TrnTokenTrn { get; init; }
    OneLoginUserNotConnectingReason? NotConnectingReason { get; init; }
    string? NotConnectingAdditionalDetails { get; init; }
    string[][]? VerifiedOrStatedNames { get; }
    DateOnly[]? VerifiedOrStatedDatesOfBirth { get; }
}

public enum OneLoginUserNotConnectingReason
{
    [Display(Name = "There is no matching record")]
    NoMatchingRecord,
    [Display(Name = "The details only partly match a record")]
    DetailsOnlyPartlyMatch,
    [Display(Name = "Another reason")]
    AnotherReason
}
