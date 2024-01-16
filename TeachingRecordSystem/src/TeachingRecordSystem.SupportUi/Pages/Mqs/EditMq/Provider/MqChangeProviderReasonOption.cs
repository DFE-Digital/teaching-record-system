using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

public enum MqChangeProviderReasonOption
{
    [Display(Name = "Incorrect training provider")]
    IncorrectTrainingProvider,
    [Display(Name = "Change of training provider")]
    ChangeOfTrainingProvider,
    [Display(Name = "Unble to confirm if the data is correct")]
    UnableToConfirmIfTheDataIsCorrect,
    [Display(Name = "Another reason")]
    AnotherReason
}
