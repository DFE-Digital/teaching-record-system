using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Models.SupportTasks;

public enum OneLoginIdVerificationRejectReason
{
    [Display(Name = "The proof of identity does not match the request details")]
    ProofDoesNotMatchRequest,
    [Display(Name = "The proof of identity is unclear")]
    ProofIsUnclear,
    [Display(Name = "The proof of identity is the wrong type")]
    ProofIsWrongType,
    [Display(Name = "Another reason")]
    AnotherReason
}
