#nullable disable
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.V2.Requests;

public class GetTrnRequest
{
    [FromRoute(Name = "requestId")]
    [Description("The unique ID the TRN request was created with.")]
    public string RequestId { get; set; }
}
