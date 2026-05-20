using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

public enum MqChangeStartDateReasonOption
{
    [Display(Name = "Start date was entered incorrectly")]
    IncorrectStartDate,
    [Display(Name = "Start date has changed")]
    ChangeOfStartDate,
    UnableToConfirmIfTheDataIsCorrect,
    [Display(Name = "Another reason")]
    AnotherReason
}
