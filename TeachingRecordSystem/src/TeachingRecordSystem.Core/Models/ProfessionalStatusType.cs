using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Models;

public enum ProfessionalStatusType
{
    [Display(Name = "Qualified Teacher Status")]
    QualifiedTeacherStatus = 0,
    [Display(Name = "Early Years Teacher Status")]
    EarlyYearsTeacherStatus = 1,
    [Display(Name = "Early Years Professional Status")]
    EarlyYearsProfessionalStatus = 2,
    [Display(Name = "Partial Qualified Teacher Status")]
    PartialQualifiedTeacherStatus = 3
}
