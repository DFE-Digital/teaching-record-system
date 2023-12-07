using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

public enum MqDeletionReasonOption
{
    [Display(Name = "Added in error")]
    AddedInError,
    [Display(Name = "Provider request")]
    ProviderRequest,
    [Display(Name = "Unable to confirm if the data is correct")]
    UnableToConfirmIfTheDataIsCorrect,
    [Display(Name = "Another reason")]
    AnotherReason
}
