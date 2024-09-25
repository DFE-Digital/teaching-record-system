using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

public enum AlertChangeStartDateReasonOption
{
    [Display(Name = "Incorrect start date")]
    IncorrectStartDate,
    [Display(Name = "Change of start date")]
    ChangeOfStartDate,
    [Display(Name = "Another reason")]
    AnotherReason
}
