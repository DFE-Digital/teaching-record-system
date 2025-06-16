using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.Create;

public enum CreateReasonOption
{
    [Display(Name = "They were awarded a mandatory qualification")]
    MandatoryQualification,
    [Display(Name = "They received an alert")]
    AlertReceived,
    [Display(Name = "They started university late")]
    UniversityLateStarter,
    [Display(Name = "Another reason")]
    AnotherReason
}
