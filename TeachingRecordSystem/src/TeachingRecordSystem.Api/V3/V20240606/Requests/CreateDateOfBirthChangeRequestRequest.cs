namespace TeachingRecordSystem.Api.V3.V20240606.Requests;

public record CreateDateOfBirthChangeRequestRequest
{
    public string? EmailAddress { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string EvidenceFileUrl { get; init; }
}
