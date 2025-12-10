namespace TeachingRecordSystem.Core.Services.OneLogin;

public record SetUserVerifiedOptions
{
    public required string OneLoginUserSubject { get; init; }
    public required OneLoginUserVerificationRoute VerificationRoute { get; init; }
    public required DateOnly[] VerifiedDatesOfBirth { get; init; }
    public required string[][] VerifiedNames { get; init; }
}
