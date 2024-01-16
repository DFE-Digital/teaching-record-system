using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

public enum MqChangeStartDateReasonOption
{
    [Display(Name = "Incorrect start date")]
    IncorrectStartDate,
    [Display(Name = "Change of start date")]
    ChangeOfStartDate,
    [Display(Name = "Unble to confirm if the data is correct")]
    UnableToConfirmIfTheDataIsCorrect,
    [Display(Name = "Another reason")]
    AnotherReason
}
