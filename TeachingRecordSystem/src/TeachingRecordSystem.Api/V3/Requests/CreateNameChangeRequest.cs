using MediatR;

namespace TeachingRecordSystem.Api.V3.Requests;

public record CreateNameChangeRequest : IRequest
{
    public required string Trn { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string EvidenceFileUrl { get; init; }
}
