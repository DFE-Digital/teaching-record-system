using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.Persons;

public enum PersonCreateReason
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
