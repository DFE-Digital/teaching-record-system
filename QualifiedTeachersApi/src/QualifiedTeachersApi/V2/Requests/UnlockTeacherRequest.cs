#nullable disable
using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.V2.Responses;

namespace QualifiedTeachersApi.V2.Requests;

public class UnlockTeacherRequest : IRequest<UnlockTeacherResponse>
{
    [FromRoute(Name = "teacherId")]
    [Description("The ID of the teacher record to unlock")]
    public Guid TeacherId { get; set; }
}
