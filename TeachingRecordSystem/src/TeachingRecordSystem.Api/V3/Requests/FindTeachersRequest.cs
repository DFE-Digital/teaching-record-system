using System.ComponentModel;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.V3.Responses;

namespace TeachingRecordSystem.Api.V3.Requests;

public record FindTeachersRequest : IRequest<FindTeachersResponse>
{
    [QueryParam, BindFrom("findBy")]
    public FindTeachersFindBy FindBy { get; init; }
    [QueryParam, BindFrom("lastName")]
    public string? LastName { get; init; }
    [QueryParam, BindFrom("dateOfBirth")]
    public DateOnly? DateOfBirth { get; init; }

    [Description("Previous first name of person")]
    [FromQuery(Name = "previousFirstName")]
    public string? PreviousFirstName { get; set; }

    [Description("Previous last name of person")]
    [FromQuery(Name = "previousLastName")]
    public string? PreviousLastName { get; set; }
}

public enum FindTeachersFindBy
{
    LastNameAndDateOfBirth = 1
}
