using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

public enum CloseAlertReasonOption
{
    [Display(Name = "End date set")]
    EndDateSet,
    [Display(Name = "Alert period has ended")]
    AlertPeriodHasEnded,
    [Display(Name = "Alert type is no longer valid")]
    AlertTypeIsNoLongerValid,
    [Display(Name = "Another reason")]
    AnotherReason
}
