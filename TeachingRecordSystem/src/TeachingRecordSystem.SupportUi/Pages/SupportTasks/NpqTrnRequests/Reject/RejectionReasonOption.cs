using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Reject;

public enum RejectionReasonOption
{
    [Display(Name = "Evidence does not match")]
    EvidenceDoesNotMatch,
    [Display(Name = "Insufficient evidence")]
    InsufficientEvidence,
    [Display(Name = "NPQ details do not match")]
    NpqDetailsDoNotMatch,
    [Display(Name = "Another reason")]
    AnotherReason
}
