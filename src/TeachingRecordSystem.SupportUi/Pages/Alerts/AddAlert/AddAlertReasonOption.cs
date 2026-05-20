using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

public enum AddAlertReasonOption
{
    [Display(Name = "Routine notification from stakeholder")]
    RoutineNotificationFromStakeholder,
    [Display(Name = "Identified during data reconciliation with stakeholder")]
    IdentifiedDuringDataReconciliationWithStakeholder,
    [Display(Name = "Another reason")]
    AnotherReason
}
