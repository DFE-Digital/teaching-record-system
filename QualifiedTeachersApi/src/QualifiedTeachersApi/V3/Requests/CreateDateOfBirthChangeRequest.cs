using System;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V3.Requests;

public record CreateDateOfBirthChangeRequest : IRequest
{
    [SwaggerSchema(Nullable = false)]
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required string EvidenceFileName { get; init; }
    [SwaggerSchema(Nullable = false)]
    public required string EvidenceFileUrl { get; init; }
}
