using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Models.SupportTasks;

public interface IOneLoginUserMatchingData : ISupportTaskData
{
    string OneLoginUserSubject { get; init; }
    string? StatedNationalInsuranceNumber { get; init; }
    string? StatedTrn { get; init; }
    string? TrnTokenTrn { get; init; }
    string? YearQtsReceived { get; init; }
    Guid? TrainingProviderId { get; init; }
    string? TrainingProviderName { get; init; }
    Guid? SubjectId { get; init; }
    string? SubjectName { get; init; }
    OneLoginUserNotConnectingReason? NotConnectingReason { get; init; }
    string? NotConnectingAdditionalDetails { get; init; }
    string[][]? VerifiedOrStatedNames { get; }
    DateOnly[]? VerifiedOrStatedDatesOfBirth { get; }
    Guid? PersonId { get; }
}

public enum OneLoginUserNotConnectingReason
{
    [Display(Name = "There is no matching record")]
    NoMatchingRecord,
    [Obsolete("This option is no longer presented to users but may exist in historical data")]
    [Display(Name = "The details only partly match a record")]
    DetailsOnlyPartlyMatch,
    [Display(Name = "Another reason")]
    AnotherReason
}
