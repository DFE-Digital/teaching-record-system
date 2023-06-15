using MediatR;
using TeachingRecordSystem.Api.V3.Responses;

namespace TeachingRecordSystem.Api.V3.Requests;

public record GetInductionCertificateRequest : IRequest<GetCertificateResponse?>
{
    public required string Trn { get; init; }
}
