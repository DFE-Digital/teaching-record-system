#nullable disable
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.V2.Requests;

public class GetTeacherRequest
{
    [FromRoute(Name = "trn")]
    public string Trn { get; set; }

    [FromQuery(Name = "includeInactive")]
    public bool? IncludeInactive { get; set; }
}
