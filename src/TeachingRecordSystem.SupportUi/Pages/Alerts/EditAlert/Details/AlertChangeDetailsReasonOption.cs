using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

public enum AlertChangeDetailsReasonOption
{
    [Display(Name = "The details were incorrect")]
    IncorrectDetails,
    [Display(Name = "The details have changed")]
    ChangeOfDetails,
    [Display(Name = "Another reason")]
    AnotherReason
}
