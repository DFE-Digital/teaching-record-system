using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

public enum ReactivateReasonOption
{
    [Display(Name = "The record was deactivated by mistake")]
    DeactivatedByMistake,
    [Display(Name = "Another reason")]
    AnotherReason
}
