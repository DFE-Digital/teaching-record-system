using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Models.SupportTasks;

public enum CaseRejectionReasonOption
{
    [Display(Name = "Request and proof don’t match")]
    RequestAndProofDontMatch,
    [Display(Name = "Wrong type of document")]
    WrongTypeOfDocument,
    [Display(Name = "Image quality")]
    ImageQuality,
    [Display(Name = "Change no longer required")]
    ChangeNoLongerRequired
}
