using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.Persons;

public enum PersonDeactivateReason
{
    [Display(Name = "The record holder died")]
    RecordHolderDied,
    [Display(Name = "There is a problem with the record")]
    ProblemWithTheRecord,
    [Display(Name = "Another reason")]
    AnotherReason
}
