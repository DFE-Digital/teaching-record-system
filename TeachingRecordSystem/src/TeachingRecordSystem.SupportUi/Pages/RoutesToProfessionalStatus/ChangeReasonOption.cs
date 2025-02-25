using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

public enum ChangeReasonOption
{
    [Display(Name = "Data loss or incomplete information")]
    IncompleteDetails,
    [Display(Name = "New information received")]
    NewInformation,
    [Display(Name = "No longer exempt from induction")]
    NoLongerExempt,
    [Display(Name = "Another reason")]
    AnotherReason
}
