namespace TeachingRecordSystem.Api.V3.V20240606.Requests;

public record CreateNameChangeRequestRequest
{
    public string? EmailAddress { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string EvidenceFileUrl { get; init; }
}
