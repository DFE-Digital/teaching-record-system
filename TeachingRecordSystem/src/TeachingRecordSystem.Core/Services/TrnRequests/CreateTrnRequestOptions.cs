namespace TeachingRecordSystem.Core.Services.TrnRequests;

public record CreateTrnRequestOptions
{
    public required bool TryResolve { get; init; }

    public required Guid ApplicationUserId { get; init; }
    public required string RequestId { get; init; }
    public required CreateTrnRequestOptionsOneLoginUserInfo? OneLoginUserInfo { get; init; }
    public required string EmailAddress { get; init; }
    public required string? FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string? LastName { get; init; }
    public string? PreviousFirstName { get; init; }
    public string? PreviousMiddleName { get; init; }
    public string? PreviousLastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
    public bool? NpqWorkingInEducationalSetting { get; init; }
    public string? NpqApplicationId { get; init; }
    public string? NpqName { get; init; }
    public string? NpqTrainingProvider { get; init; }
    public Guid? NpqEvidenceFileId { get; init; }
    public string? NpqEvidenceFileName { get; init; }
    public string? WorkEmailAddress { get; init; }
}

public record CreateTrnRequestOptionsOneLoginUserInfo(string OneLoginUserSubject, bool IdentityVerified);
