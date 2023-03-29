using System.ComponentModel.DataAnnotations;
using MediatR;
using QualifiedTeachersApi.V3.Responses;

namespace QualifiedTeachersApi.V3.Requests;

public record GetTeacherRequest : IRequest<GetTeacherResponse>
{
    [RegularExpression(@"^\d{7}$", ErrorMessage = "TRN must be 7 digits")]
    public required string Trn { get; init; }
}
