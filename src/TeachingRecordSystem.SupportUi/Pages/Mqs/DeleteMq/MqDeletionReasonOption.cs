using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

public enum MqDeletionReasonOption
{
    [Display(Name = "It was added in error")]
    AddedInError,
    [Display(Name = "It was requested by a provider")]
    ProviderRequest,
    [Display(Name = "Another reason")]
    AnotherReason
}
