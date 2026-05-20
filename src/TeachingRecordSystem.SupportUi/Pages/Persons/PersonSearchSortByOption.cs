using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.Persons;

public enum PersonSearchSortByOption
{
    [Display(Name = "Name")]
    Name,
    [Display(Name = "Date of birth")]
    DateOfBirth,
    [Display(Name = "One Login email address")]
    OneLoginEmailAddress,
    [Display(Name = "TRN")]
    Trn,
    [Display(Name = "National Insurance number")]
    NationalInsuranceNumber,
    [Display(Name = "Record status")]
    RecordStatus
}
