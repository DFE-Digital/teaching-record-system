using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

public enum MqChangeEndDateReasonOption
{
    [Display(Name = "Incorrect end date")]
    IncorrectEndDate,
    [Display(Name = "Change of end date")]
    ChangeOfEndDate,
    [Display(Name = "Unble to confirm if the data is correct")]
    UnableToConfirmIfTheDataIsCorrect,
    [Display(Name = "Another reason")]
    AnotherReason
}

