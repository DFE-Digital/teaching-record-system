using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.Persons;

public enum PersonInductionChangeReason
{
    [Display(Name = "Data loss or incomplete information")]
    IncompleteDetails,
    [Display(Name = "New information received")]
    NewInformation,
    [Display(Name = "No longer exempt from induction")]
    NoLongerExempt,
    [Display(Name = "Another reason")]
    AnotherReason
}
