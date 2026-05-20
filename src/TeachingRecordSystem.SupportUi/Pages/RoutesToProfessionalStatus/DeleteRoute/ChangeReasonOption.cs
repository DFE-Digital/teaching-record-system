using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

public enum ChangeReasonOption
{
    [Display(Name = "It was created in error")]
    CreatedInError,
    [Display(Name = "No longer holds QTLS status")]
    RemovedQtlsStatus,
    [Display(Name = "Another reason")]
    AnotherReason
}
