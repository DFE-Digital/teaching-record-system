using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.InductionExemptions;

public enum ExemptionReasonCategory
{
    [Display(Name = "Miscellaneous exemptions")]
    Miscellaneous = 1,
    [Display(Name = "Exemptions for historical qualification routes")]
    HistoricalQualificationRoute = 2,
    [Display(Name = "Induction completed outside England")]
    InductionCompletedOutsideEngland = 3
}
