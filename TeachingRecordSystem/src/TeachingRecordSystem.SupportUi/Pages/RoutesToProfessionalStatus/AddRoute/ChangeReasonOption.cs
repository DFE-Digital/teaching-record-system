using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public enum ChangeReasonOption
{
    [Display(Name = "New Information Received")]
    NewInformationReceived,
    [Display(Name = "Manually added for Register")]
    AddedForRegister,
    [Display(Name = "Manually added for Apply for QTS")]
    AddedForApplyQts,
    [Display(Name = "Another reason")]
    AnotherReason
}
