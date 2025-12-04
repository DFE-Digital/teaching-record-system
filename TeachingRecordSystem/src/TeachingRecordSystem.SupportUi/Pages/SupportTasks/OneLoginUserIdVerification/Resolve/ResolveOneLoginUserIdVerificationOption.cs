using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

public enum ResolveOneLoginUserIdVerificationOption
{
    [Display(Name = "Yes, find a matching record")]
    Verified,
    [Display(Name = "No, reject this request")]
    NotVerified,
}
