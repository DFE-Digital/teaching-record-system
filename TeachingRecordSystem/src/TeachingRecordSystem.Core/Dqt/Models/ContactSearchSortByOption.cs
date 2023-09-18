using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Dqt.Models;

public enum ContactSearchSortByOption
{
    [Display(Name = "Last name (A-Z)")]
    LastNameAscending,
    [Display(Name = "Last name (Z-A)")]
    LastNameDescending,
    [Display(Name = "First name (A-Z)")]
    FirstNameAscending,
    [Display(Name = "First name (Z-A)")]
    FirstNameDescending,
    [Display(Name = "Date of birth (ascending)")]
    DateOfBirthAscending,
    [Display(Name = "Date of birth (descending)")]
    DateOfBirthDescending
}
