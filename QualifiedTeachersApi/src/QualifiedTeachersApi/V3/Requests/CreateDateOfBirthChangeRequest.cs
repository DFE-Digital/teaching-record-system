using MediatR;

namespace QualifiedTeachersApi.V3.Requests;

public record CreateDateOfBirthChangeRequest : IRequest
{
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string EvidenceFileUrl { get; init; }
}
