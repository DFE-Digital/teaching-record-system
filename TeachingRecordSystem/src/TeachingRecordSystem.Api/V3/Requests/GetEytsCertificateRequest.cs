using MediatR;
using TeachingRecordSystem.Api.V3.Responses;

namespace TeachingRecordSystem.Api.V3.Requests;

public record GetEytsCertificateRequest : IRequest<GetCertificateResponse?>
{
    public required string Trn { get; init; }
}
