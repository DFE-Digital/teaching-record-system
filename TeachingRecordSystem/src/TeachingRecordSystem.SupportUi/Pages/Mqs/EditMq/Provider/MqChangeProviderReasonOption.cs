using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

public enum MqChangeProviderReasonOption
{
    [Display(Name = "Training provider was entered incorrectly")]
    IncorrectTrainingProvider,
    [Display(Name = "Training provider has changed")]
    ChangeOfTrainingProvider,
    [Display(Name = "Another reason")]
    AnotherReason
}
