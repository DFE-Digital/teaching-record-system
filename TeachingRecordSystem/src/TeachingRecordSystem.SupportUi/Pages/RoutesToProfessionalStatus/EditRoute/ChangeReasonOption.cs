using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

public enum ChangeReasonOption
{
    [Display(Name = "Information was missing or incorrect")]
    IncompleteDetails,
    [Display(Name = "New information was received")]
    NewInformation,
    [Display(Name = "Induction exemption no longer applies")]
    NoLongerExemptFromInduction,
    [Display(Name = "Another reason")]
    AnotherReason
}
