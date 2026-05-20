using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Models.SupportTasks;

public record OneLoginUserIdVerificationData : IOneLoginUserMatchingData
{
    public required string OneLoginUserSubject { get; init; }
    public required string StatedFirstName { get; init; }
    public required string StatedLastName { get; init; }
    public required DateOnly StatedDateOfBirth { get; init; }
    public required string? StatedNationalInsuranceNumber { get; init; }
    public required string? StatedTrn { get; init; }
    public required Guid EvidenceFileId { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string? TrnTokenTrn { get; init; }
    public required Guid ClientApplicationUserId { get; init; }
    public bool? Verified { get; init; }
    public Guid? PersonId { get; init; }
    public OneLoginUserIdVerificationOutcome Outcome { get; init; }
    public OneLoginIdVerificationRejectReason? RejectReason { get; init; }
    public string? RejectionAdditionalDetails { get; init; }
    public OneLoginUserNotConnectingReason? NotConnectingReason { get; init; }
    public string? NotConnectingAdditionalDetails { get; init; }
    public string[][]? VerifiedOrStatedNames => [[StatedFirstName, StatedLastName]];
    public DateOnly[]? VerifiedOrStatedDatesOfBirth => [StatedDateOfBirth];
}

public enum OneLoginUserIdVerificationOutcome
{
    NotVerified = 0,
    VerifiedOnlyWithMatches = 1,
    VerifiedOnlyWithoutMatches = 2,
    VerifiedAndConnected = 3
}

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
