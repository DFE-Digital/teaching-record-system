namespace TeachingRecordSystem.Api.V3.V20240101.Requests;

public record CreateDateOfBirthChangeRequestRequest
{
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string EvidenceFileUrl { get; init; }
}
