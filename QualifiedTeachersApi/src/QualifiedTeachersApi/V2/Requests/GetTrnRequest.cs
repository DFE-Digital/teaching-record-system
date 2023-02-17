using MediatR;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.V2.Responses;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V2.Requests;

public class GetTrnRequest : IRequest<TrnRequestInfo>
{
    [FromRoute(Name = "requestId")]
    [SwaggerParameter(description: "The unique ID the TRN request was created with.")]
    public string RequestId { get; set; }
}
