using System.ComponentModel.DataAnnotations;
using MediatR;
using QualifiedTeachersApi.V3.Responses;

namespace QualifiedTeachersApi.V3.Requests;

public record GetTeacherRequest : IRequest<GetTeacherResponse>
{
    public required string Trn { get; init; }
}
