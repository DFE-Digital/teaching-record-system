using System.ComponentModel;

namespace TeachingRecordSystem.Core.Models;

public enum ProfessionalStatusStatus
{
    [Description("In training")]
    InTraining = 0,
    [Description("Awarded")]
    Awarded = 1,
    [Description("Deferred")]
    Deferred = 2,
    [Description("Deferred for skills test")]
    DeferredForSkillsTest = 3,
    [Description("Failed")]
    Failed = 4,
    [Description("Withdrawn")]
    Withdrawn = 5,
    [Description("Under assessment")]
    UnderAssessment = 6,
    [Description("Approved")]
    Approved = 7
}
