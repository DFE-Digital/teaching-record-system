using MediatR;
using TeachingRecordSystem.Api.V3.Responses;

namespace TeachingRecordSystem.Api.V3.Requests;

public class GetTrnRequest : IRequest<TrnRequestInfo?>
{
    public required Guid RequestId { get; set; }
}
