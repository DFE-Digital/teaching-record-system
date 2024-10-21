using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

public enum AlertChangeLinkReasonOption
{
    [Display(Name = "Incorrect link")]
    IncorrectLink,
    [Display(Name = "Change of link")]
    ChangeOfLink,
    [Display(Name = "Another reason")]
    AnotherReason
}
