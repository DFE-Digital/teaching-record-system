using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

public enum ResolveOneLoginUserMatchingVerifyOption
{
    [Display(Name = "Yes, find a matching record")]
    Verified,
    [Display(Name = "No, reject this request")]
    NotVerified,
}
