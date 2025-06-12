using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Models;

public enum ProfessionalStatusType
{
    [Display(Name = "Qualified teacher status")]
    QualifiedTeacherStatus = 0,
    [Display(Name = "Early years teacher status")]
    EarlyYearsTeacherStatus = 1,
    [Display(Name = "Early years professional status")]
    EarlyYearsProfessionalStatus = 2,
    [Display(Name = "Partial qualified teacher status")]
    PartialQualifiedTeacherStatus = 3
}
