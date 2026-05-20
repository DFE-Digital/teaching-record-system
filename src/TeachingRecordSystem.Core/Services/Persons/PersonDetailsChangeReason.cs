using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.Persons;

public enum PersonDetailsChangeReason
{
    [Display(Name = "Data loss or incomplete information")]
    IncompleteDetails,
    [Display(Name = "New information received")]
    NewInformation,
    [Display(Name = "Another reason")]
    AnotherReason
}
