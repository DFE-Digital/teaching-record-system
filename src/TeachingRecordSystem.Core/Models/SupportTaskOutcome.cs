using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TeachingRecordSystem.Core.Models;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public enum SupportTaskOutcome
{
    [Display(Name = "Approved")]
    ChangeNameRequest_Approved = 1,

    [Display(Name = "Rejected")]
    ChangeNameRequest_Rejected = 2,

    [Display(Name = "Cancelled")]
    ChangeNameRequest_Cancelled = 3,

    [Display(Name = "Approved")]
    ChangeDateOfBirthRequest_Approved = 4,

    [Display(Name = "Rejected")]
    ChangeDateOfBirthRequest_Rejected = 5,

    [Display(Name = "Cancelled")]
    ChangeDateOfBirthRequest_Cancelled = 6,

    [Display(Name = "Not verified")]
    OneLoginUserIdVerification_NotVerified = 7,

    [Display(Name = "Verified and not connected")]
    OneLoginUserIdVerification_VerifiedOnlyWithMatches = 8,

    [Display(Name = "Verified (no matches)")]
    OneLoginUserIdVerification_VerifiedOnlyWithoutMatches = 9,

    [Display(Name = "Verified and connected")]
    OneLoginUserIdVerification_VerifiedAndConnected = 10,

    [Display(Name = "Not connected")]
    OneLoginUserRecordMatching_NotConnecting = 11,

    [Display(Name = "No matches")]
    OneLoginUserRecordMatching_NoMatches = 12,

    [Display(Name = "Connected")]
    OneLoginUserRecordMatching_Connected = 13,

    [Display(Name = "Resolved with existing record")]
    TrnRequest_ResolvedWithExistingPerson = 14,

    [Display(Name = "Resolved with new record")]
    TrnRequest_ResolvedWithNewPerson = 15,

    [Display(Name = "Checks completed")]
    TrnRequestManualChecksNeeded_Completed = 16,

    [Display(Name = "Resolved (record merged)")]
    TeacherPensionsPotentialDuplicate_ResolvedWithMerge = 17,

    [Display(Name = "Resolved (record kept)")]
    TeacherPensionsPotentialDuplicate_ResolvedWithoutMerge = 18,

    [Display(Name = "Resolved with existing record")]
    NpqTrnRequest_ResolvedWithExistingPerson = 19,

    [Display(Name = "Resolved with new record")]
    NpqTrnRequest_ResolvedWithNewPerson = 20,

    [Display(Name = "Rejected")]
    NpqTrnRequest_Rejected = 21,
}
