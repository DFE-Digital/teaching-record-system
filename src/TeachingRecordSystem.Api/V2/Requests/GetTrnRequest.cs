#nullable disable
using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Requests;

public class GetTrnRequest : IRequest<TrnRequestInfo>
{
    [FromRoute(Name = "requestId")]
    [Description("The unique ID the TRN request was created with.")]
    public string RequestId { get; set; }
}
