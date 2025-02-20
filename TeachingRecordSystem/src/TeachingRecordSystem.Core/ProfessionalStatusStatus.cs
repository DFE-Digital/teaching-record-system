using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core;

public enum ProfessionalStatusStatus
{
    [Display(Name = "In training")]
    InTraining = 0,
    [Display(Name = "Awarded")]
    Awarded = 1,
    [Display(Name = "Deferred")]
    Deferred = 2,
    [Display(Name = "Deferred for skills tests")]
    DeferredForSkillsTest = 3,
    [Display(Name = "Failed")]
    Failed = 4,
    [Display(Name = "Withdrawn")]
    Withdrawn = 5,
    [Display(Name = "Under assessment")]
    UnderAssessment = 6,
    [Display(Name = "Approved")]
    Approved = 7
}
