using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DqtApi.V2.ApiModels;
using DqtApi.V2.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi.V2.Requests
{
    public class GetOrCreateTrnRequest : IRequest<TrnRequestInfo>
    {
        [FromRoute(Name = "requestId")]
        [SwaggerParameter(Description =
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
    }

    public class GetOrCreateTrnRequestQualification
    {
        public string ProviderUkprn { get; set; }
        public string CountryCode { get; set; }
        public string Subject { get; set; }
        public ClassDivision? Class { get; set; }
        public DateOnly? Date { get; set; }
    }
}
