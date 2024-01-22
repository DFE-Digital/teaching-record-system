using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

public enum MqChangeSpecialismReasonOption
{
    [Display(Name = "Incorrect specialism")]
    IncorrectSpecialism,
    [Display(Name = "Change of specialism")]
    ChangeOfSpecialism,
    [Display(Name = "Unble to confirm if the data is correct")]
    UnableToConfirmIfTheDataIsCorrect,
    [Display(Name = "Another reason")]
    AnotherReason
}
