using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

public enum AddMqReasonOption
{
    [Display(Name = "New information was received")]
    NewInformationReceived,
    [Display(Name = "Another reason")]
    AnotherReason
}
