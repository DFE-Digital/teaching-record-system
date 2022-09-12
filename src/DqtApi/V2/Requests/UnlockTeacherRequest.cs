using System;
using DqtApi.V2.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi.V2.Requests
{
    public class UnlockTeacherRequest : IRequest<UnlockTeacherResponse>
    {
        [FromRoute(Name = "teacherId")]
        [SwaggerParameter(description: "The ID of the teacher record to unlock")]
        public Guid TeacherId { get; set; }
    }
}
