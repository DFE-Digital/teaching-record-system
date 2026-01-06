using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.InductionExemptions;

public enum ExemptionReasonCategory
{
    [Display(Name = "General exemptions")]
    Miscellaneous = 1,
    [Display(Name = "Historical qualification route exemptions")]
    HistoricalQualificationRoute = 2,
    [Display(Name = "Induction completed outside England")]
    InductionCompletedOutsideEngland = 3
}
