using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Models.SupportTasks;

public enum OneLoginIdVerificationNotConnectingReason
{
    [Display(Name = "There is no matching record")]
    NoMatchingRecord,
    [Display(Name = "The details only partly match a record")]
    DetailsOnlyPartlyMatch,
    [Display(Name = "Another reason")]
    AnotherReason
}
