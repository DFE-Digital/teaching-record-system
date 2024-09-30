using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.EndDate;

public enum AlertChangeEndDateReasonOption
{
    [Display(Name = "Incorrect end date")]
    IncorrectEndDate,
    [Display(Name = "Change of end date")]
    ChangeOfEndDate,
    [Display(Name = "Another reason")]
    AnotherReason
}
