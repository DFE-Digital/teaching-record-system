#nullable disable
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.V2.Responses;

namespace QualifiedTeachersApi.V2.Requests;

public class GetTeacherRequest : IRequest<GetTeacherResponse>
{
    [FromRoute(Name = "trn")]
    public string Trn { get; set; }

    [FromQuery(Name = "includeInactive")]
    public bool? IncludeInactive { get; set; }
}
