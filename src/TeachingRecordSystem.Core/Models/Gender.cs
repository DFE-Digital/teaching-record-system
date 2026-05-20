using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Models;

public enum Gender
{
    [Display(Name = "Male")]
    Male = 1,
    [Display(Name = "Female")]
    Female = 2,
    [Display(Name = "Other")]
    Other = 3,
    [Display(Name = "Not available")]
    NotAvailable = 4
}
