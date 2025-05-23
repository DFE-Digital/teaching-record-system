using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

public enum EditDetailsChangeReasonOption
{
    [Display(Name = "Data loss or incomplete information")]
    IncompleteDetails,
    [Display(Name = "New information received")]
    NewInformation,
    [Display(Name = "Another reason")]
    AnotherReason
}
