using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

public enum MqChangeEndDateReasonOption
{
    [Display(Name = "End date was entered incorrectly")]
    IncorrectEndDate,
    [Display(Name = "End date has changed")]
    ChangeOfEndDate,
    [Display(Name = "Another reason")]
    AnotherReason
}

