using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.V2.Responses;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V2.Requests;

public class UnlockTeacherRequest : IRequest<UnlockTeacherResponse>
{
    [FromRoute(Name = "teacherId")]
    [SwaggerParameter(description: "The ID of the teacher record to unlock")]
    public Guid TeacherId { get; set; }
}
