using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

public enum DeleteAlertReasonOption
{
    [Display(Name = "It was added in error")]
    AddedInError,
    [Display(Name = "Another reason")]
    AnotherReason
}
