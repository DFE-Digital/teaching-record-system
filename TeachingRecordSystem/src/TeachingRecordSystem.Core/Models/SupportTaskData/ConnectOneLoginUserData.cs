namespace TeachingRecordSystem.Core.Models.SupportTaskData;

public record ConnectOneLoginUserData
{
    public required bool Verified { get; init; }
    public required string OneLoginUserSubject { get; init; }
    public required string OneLoginUserEmail { get; init; }
    public required string[][]? VerifiedNames { get; init; }
    public required DateOnly[]? VerifiedDatesOfBirth { get; init; }
    public required string? StatedNationalInsuranceNumber { get; init; }
    public required string? StatedTrn { get; init; }
    public required string? TrnTokenTrn { get; init; }
    public required Guid ClientApplicationUserId { get; init; }
    public Guid? PersonId { get; set; }
}
