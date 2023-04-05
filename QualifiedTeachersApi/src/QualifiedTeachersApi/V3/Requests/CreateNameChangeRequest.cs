#nullable disable
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V3.Requests;

public record CreateNameChangeRequest : IRequest
{
    [SwaggerSchema(Nullable = false)]
    public required string Trn { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required string LastName { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required string EvidenceFileName { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required string EvidenceFileUrl { get; init; }
}
