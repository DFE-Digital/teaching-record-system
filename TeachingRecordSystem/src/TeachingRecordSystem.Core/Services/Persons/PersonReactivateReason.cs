using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.Persons;

public enum PersonReactivateReason
{
    [Display(Name = "The record was deactivated by mistake")]
    DeactivatedByMistake,
    [Display(Name = "Another reason")]
    AnotherReason
}
