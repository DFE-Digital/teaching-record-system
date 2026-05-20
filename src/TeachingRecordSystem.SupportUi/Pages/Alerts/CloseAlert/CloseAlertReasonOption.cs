using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

public enum CloseAlertReasonOption
{
    [Display(Name = "The end date is set")]
    EndDateSet,
    [Display(Name = "The alert period has ended")]
    AlertPeriodHasEnded,
    [Display(Name = "The alert type is no longer valid")]
    AlertTypeIsNoLongerValid,
    [Display(Name = "Another reason")]
    AnotherReason
}
