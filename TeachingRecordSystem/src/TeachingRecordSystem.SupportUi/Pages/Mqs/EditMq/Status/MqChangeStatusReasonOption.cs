using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

public enum MqChangeStatusReasonOption
{
    [Display(Name = "Incorrect status")]
    IncorrectStatus,
    [Display(Name = "Change of status")]
    ChangeOfStatus,
    [Display(Name = "Unble to confirm if the data is correct")]
    UnableToConfirmIfTheDataIsCorrect,
    [Display(Name = "Another reason")]
    AnotherReason
}
