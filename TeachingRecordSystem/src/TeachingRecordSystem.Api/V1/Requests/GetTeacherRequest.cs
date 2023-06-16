#nullable disable
using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.V1.Responses;

namespace TeachingRecordSystem.Api.V1.Requests;

public class GetTeacherRequest : IRequest<GetTeacherResponse>
{
    [FromRoute(Name = "trn")]
    public string Trn { get; set; }

    [FromQuery(Name = "birthdate"), Required]
    public DateTime? BirthDate { get; set; }

    [FromQuery(Name = "nino")]
    public string NationalInsuranceNumber { get; set; }
}
