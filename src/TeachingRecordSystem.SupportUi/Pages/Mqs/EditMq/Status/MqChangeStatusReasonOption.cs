using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

public enum MqChangeStatusReasonOption
{
    [Display(Name = "Status was entered incorrectly")]
    IncorrectStatus,
    [Display(Name = "Status has changed")]
    ChangeOfStatus,
    [Display(Name = "Another reason")]
    AnotherReason
}
