using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

public enum ResolveOneLoginUserMatchingVerifyOption
{
    [Display(Name = "Yes, verify and find a matching record (if applicable)")]
    Verified,
    [Display(Name = "No, reject this request")]
    NotVerified,
}
