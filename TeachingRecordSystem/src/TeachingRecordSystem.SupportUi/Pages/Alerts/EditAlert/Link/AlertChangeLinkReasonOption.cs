using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

public enum AlertChangeLinkReasonOption
{
    [Display(Name = "The link was incorrect")]
    IncorrectLink,
    [Display(Name = "The link was changed")]
    ChangeOfLink,
    [Display(Name = "Another reason")]
    AnotherReason
}
