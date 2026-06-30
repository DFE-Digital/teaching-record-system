using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.OneLogin;

public enum DisconnectPersonStayVerified
{
    [Display(Name = "Yes, keep this GOV.UK One Login verified")]
    Yes,
    [Display(Name = "No, remove verification from this GOV.UK One Login")]
    No
}
