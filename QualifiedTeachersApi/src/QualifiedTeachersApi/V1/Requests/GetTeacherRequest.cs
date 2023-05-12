#nullable disable
using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.V1.Responses;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V1.Requests;

public class GetTeacherRequest : IRequest<GetTeacherResponse>
{
    [FromRoute(Name = "trn")]
    public string Trn { get; set; }

    [FromQuery(Name = "birthdate"), SwaggerParameter(Required = true), SwaggerSchema(Format = "date")]
    public DateTime? BirthDate { get; set; }

    [FromQuery(Name = "nino")]
    public string NationalInsuranceNumber { get; set; }
}
