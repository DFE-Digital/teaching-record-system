namespace TeachingRecordSystem.Api.V3.V20240412.Requests;

[AutoMap(typeof(Core.Operations.CreateDateOfBirthChangeRequestCommand), ReverseMap = true)]
public record CreateDateOfBirthChangeRequestRequest
{
    public string? Email { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string EvidenceFileUrl { get; init; }
}
