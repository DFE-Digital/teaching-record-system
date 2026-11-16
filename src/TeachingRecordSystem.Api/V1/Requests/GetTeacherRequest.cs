#nullable disable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.V1.Requests;

public class GetTeacherRequest
{
    [FromRoute(Name = "trn")]
    public string Trn { get; set; }

    [FromQuery(Name = "birthdate"), Required]
    public DateTime? BirthDate { get; set; }

    [FromQuery(Name = "nino")]
    public string NationalInsuranceNumber { get; set; }
}
