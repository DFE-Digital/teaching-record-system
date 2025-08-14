using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

public enum DeactivateReasonOption
{
    [Display(Name = "The record holder died")]
    RecordHolderDied,
    [Display(Name = "There is a problem with the record")]
    ProblemWithTheRecord,
    [Display(Name = "Another reason")]
    AnotherReason
}
