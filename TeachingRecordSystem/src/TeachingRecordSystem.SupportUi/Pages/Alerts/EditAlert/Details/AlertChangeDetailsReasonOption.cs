using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

public enum AlertChangeDetailsReasonOption
{
    [Display(Name = "Incorrect details")]
    IncorrectDetails,
    [Display(Name = "Change of details")]
    ChangeOfDetails,
    [Display(Name = "Another reason")]
    AnotherReason
}
