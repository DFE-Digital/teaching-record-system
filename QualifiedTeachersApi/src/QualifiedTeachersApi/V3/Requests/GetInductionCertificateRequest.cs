#nullable disable
using MediatR;
using QualifiedTeachersApi.V3.Responses;

namespace QualifiedTeachersApi.V3.Requests;

public record GetInductionCertificateRequest : IRequest<GetCertificateResponse>
{
    public required string Trn { get; init; }
}
