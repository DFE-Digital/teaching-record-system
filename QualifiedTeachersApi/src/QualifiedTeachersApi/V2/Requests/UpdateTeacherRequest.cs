#nullable disable
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.V2.ApiModels;

namespace QualifiedTeachersApi.V2.Requests;

public class UpdateTeacherRequest : IRequest
{
    [Description("The TRN of the teacher to update")]
    [FromRoute(Name = "trn")]
    [JsonIgnore]
    public string Trn { get; set; }

    [Required]
    public UpdateTeacherRequestInitialTeacherTraining InitialTeacherTraining { get; set; }
    [Required]
    public UpdateTeacherRequestQualification Qualification { get; set; }

    [Required]
    [FromQuery(Name = "birthdate"), Description("DoB of teacher")]
    [JsonIgnore]
    public DateOnly? BirthDate { get; set; }

    public string HusId { get; set; }
}

public class UpdateTeacherRequestInitialTeacherTraining
{
    [Required]
    public string ProviderUkprn { get; set; }
    [Required]
    public DateOnly? ProgrammeStartDate { get; set; }
    [Required]
    public DateOnly? ProgrammeEndDate { get; set; }
    [Required]
    public IttProgrammeType? ProgrammeType { get; set; }
    public string Subject1 { get; set; }
    public string Subject2 { get; set; }
    public string Subject3 { get; set; }
    public int? AgeRangeFrom { get; set; }
    public int? AgeRangeTo { get; set; }
    public IttQualificationType? IttQualificationType { get; set; }
    public IttQualificationAim? IttQualificationAim { get; set; }
    public string TrainingCountryCode { get; set; }
    public IttOutcome? Outcome { get; set; }
}

public class UpdateTeacherRequestQualification
{
    public string ProviderUkprn { get; set; }
    public string CountryCode { get; set; }
    public string Subject { get; set; }
    public string Subject2 { get; set; }
    public string Subject3 { get; set; }
    public ClassDivision? Class { get; set; }
    public DateOnly? Date { get; set; }
    public HeQualificationType? HeQualificationType { get; set; }
}
