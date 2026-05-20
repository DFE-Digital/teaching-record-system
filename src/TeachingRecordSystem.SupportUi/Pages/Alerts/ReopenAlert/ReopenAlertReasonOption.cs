using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

public enum ReopenAlertReasonOption
{
    [Display(Name = "Closed in error")]
    ClosedInError,
    [Display(Name = "Another reason")]
    AnotherReason
}
