using System.ComponentModel;
using MediatR;
using TeachingRecordSystem.Api.V3.Responses;

namespace TeachingRecordSystem.Api.V3.Requests;

public record CreateTrnRequestBody : IRequest<TrnRequestInfo>
{
    [Description(
    "A unique ID that represents this request. " +
    "If a request has already been created with this ID then that existing record's result is returned.")]
    public required string RequestId { get; set; }
    public required TrnRequestPerson Person { get; init; }
}

