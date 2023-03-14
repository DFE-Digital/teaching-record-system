using MediatR;
using QualifiedTeachersApi.V3.Responses;

namespace QualifiedTeachersApi.V3.Requests;

public record GetEytsCertificateRequest : IRequest<GetCertificateResponse>
{
    public required string Trn { get; init; }
}
