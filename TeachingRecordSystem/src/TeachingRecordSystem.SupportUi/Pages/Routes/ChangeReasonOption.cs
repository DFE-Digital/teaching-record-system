using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Routes;

public enum ChangeReasonOption
{
    [Display(Name = "Data loss or incomplete information")]
    IncompleteDetails,
    [Display(Name = "New information received")]
    NewInformation,
    [Display(Name = "Another reason")]
    AnotherReason
}
