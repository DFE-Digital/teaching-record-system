using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.Persons;

public enum DisconnectOneLoginStayVerified
{
    [Display(Name = "Yes, keep this GOV.UK One Login verified")]
    Yes,
    [Display(Name = "No, remove verification from this GOV.UK One Login")]
    No
}
