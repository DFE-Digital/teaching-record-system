#nullable disable
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Api.V2.ApiModels;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Requests;

public class GetOrCreateTrnRequest : IRequest<TrnRequestInfo>
{
    [FromRoute(Name = "requestId")]
    [Description(
        "A unique ID that represents this request. " +
        "If a request has already been created with this ID then that existing record's result is returned.")]
    [JsonIgnore]
    public string RequestId { get; set; }

    [Required]
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public DateOnly BirthDate { get; set; }
    public string EmailAddress { get; set; }
    public GetOrCreateTrnRequestAddress Address { get; set; }
    [Required]
    public Gender GenderCode { get; set; }
    [Required]
    public GetOrCreateTrnRequestInitialTeacherTraining InitialTeacherTraining { get; set; }
    [Required]
    public GetOrCreateTrnRequestQualification Qualification { get; set; }
    public string HusId { get; set; }
    public CreateTeacherType TeacherType { get; set; }
    public CreateTeacherRecognitionRoute? RecognitionRoute { get; set; }
    public DateOnly? QtsDate { get; set; }
    public bool? InductionRequired { get; set; }
    public bool? UnderNewOverseasRegulations { get; set; }
    public string SlugId { get; set; }
    public bool IdentityVerified { get; set; }
    public string OneLoginUserSubject { get; set; }
}

public class GetOrCreateTrnRequestAddress
{
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string AddressLine3 { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
}

public class GetOrCreateTrnRequestInitialTeacherTraining
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
}

public class GetOrCreateTrnRequestQualification
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

public enum CreateTeacherType
{
    TraineeTeacher = 0,
    OverseasQualifiedTeacher = 1
}

public enum CreateTeacherRecognitionRoute
{
    Scotland = 1,
    NorthernIreland = 2,
    EuropeanEconomicArea = 3,
    OverseasTrainedTeachers = 4
}
