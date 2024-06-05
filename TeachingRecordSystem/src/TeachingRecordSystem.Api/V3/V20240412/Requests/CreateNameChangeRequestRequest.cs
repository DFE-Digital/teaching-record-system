namespace TeachingRecordSystem.Api.V3.V20240412.Requests;

[AutoMap(typeof(Core.Operations.CreateNameChangeRequestCommand), ReverseMap = true)]
public record CreateNameChangeRequestRequest
{
    public string? Email { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string EvidenceFileUrl { get; init; }
}
