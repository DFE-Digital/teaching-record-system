using MediatR;

namespace QualifiedTeachersApi.V3.Requests;

public record GetQtsCertificateRequest : IRequest<byte[]>
{
    public required string Trn { get; init; }
}
