using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

public enum AlertChangeStartDateReasonOption
{
    [Display(Name = "The start date was incorrect")]
    IncorrectStartDate,
    [Display(Name = "The start date has changed")]
    ChangeOfStartDate,
    [Display(Name = "Another reason")]
    AnotherReason
}
