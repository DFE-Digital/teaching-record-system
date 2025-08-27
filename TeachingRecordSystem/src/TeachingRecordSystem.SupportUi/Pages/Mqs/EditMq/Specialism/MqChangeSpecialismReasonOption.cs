using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

public enum MqChangeSpecialismReasonOption
{
    [Display(Name = "Specialism was entered incorrectly")]
    IncorrectSpecialism,
    [Display(Name = "Specialism has changed")]
    ChangeOfSpecialism,
    [Display(Name = "Another reason")]
    AnotherReason
}
