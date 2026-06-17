using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins;

public enum OneLoginSearchSortByOption
{
    [Display(Name = "Email")]
    Email,
    [Display(Name = "Name")]
    Name,
    [Display(Name = "Date of birth")]
    DateOfBirth,
    [Display(Name = "TRN")]
    Trn
}
