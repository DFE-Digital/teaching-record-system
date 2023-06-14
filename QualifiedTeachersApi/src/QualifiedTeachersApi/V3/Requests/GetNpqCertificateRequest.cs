using MediatR;
using QualifiedTeachersApi.V3.Responses;

namespace QualifiedTeachersApi.V3.Requests;

public record GetNpqCertificateRequest : IRequest<GetCertificateResponse?>
{
    public required Guid QualificationId { get; set; }

    public required string Trn { get; init; }
}
