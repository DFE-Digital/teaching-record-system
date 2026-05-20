using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.EndDate;

public enum AlertChangeEndDateReasonOption
{
    [Display(Name = "The end date was incorrect")]
    IncorrectEndDate,
    [Display(Name = "The end date changed")]
    ChangeOfEndDate,
    [Display(Name = "Another reason")]
    AnotherReason
}
