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

public static class GenderExtensions
{
    public static Gender? ToGender(this Contact_GenderCode? genderCode)
        => genderCode is Contact_GenderCode notNull
            ? notNull.ToGender()
            : null;

    public static Gender? ToGender(this Contact_GenderCode genderCode)
        => genderCode switch
        {
            Contact_GenderCode.Male => Gender.Male,
            Contact_GenderCode.Female => Gender.Female,
            Contact_GenderCode.Other => Gender.Other,
            Contact_GenderCode.Notavailable => Gender.NotAvailable,
            _ => null
        };

    public static Contact_GenderCode? ToContact_GenderCode(this Gender gender)
        => gender switch
        {
            Gender.Male => Contact_GenderCode.Male,
            Gender.Female => Contact_GenderCode.Female,
            Gender.Other => Contact_GenderCode.Other,
            Gender.NotAvailable => Contact_GenderCode.Notavailable,
            _ => null
        };
}
