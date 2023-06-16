using MediatR;
using TeachingRecordSystem.Api.V3.Responses;

namespace TeachingRecordSystem.Api.V3.Requests;

public record GetNpqCertificateRequest : IRequest<GetCertificateResponse?>
{
    public required Guid QualificationId { get; set; }

    public required string Trn { get; init; }
}
