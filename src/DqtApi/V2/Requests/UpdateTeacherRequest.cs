using System;
using System.ComponentModel.DataAnnotations;
using DqtApi.V2.ApiModels;
using DqtApi.V2.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi.V2.Requests
{
    public class UpdateTeacherRequest : IRequest
    {
        [SwaggerParameter(Description = "The TRN of the teacher to update")]
        [FromRoute(Name = "trn")]
        public string Trn { get; set; }

        [Required]
        public UpdateTeacherRequestInitialTeacherTraining InitialTeacherTraining { get; set; }
        [Required]
        public UpdateTeacherRequestRequestQualification Qualification { get; set; }

        [Required]
        [SwaggerParameter(description: "DoB of teacher")]
        public DateOnly BirthDate { get; set; }
    }

    public class UpdateTeacherRequestInitialTeacherTraining
    {
        public string ProviderUkprn { get; set; }
        [Required]
        public DateOnly? ProgrammeStartDate { get; set; }
        [Required]
        public DateOnly? ProgrammeEndDate { get; set; }
        [Required]
        public IttProgrammeType? ProgrammeType { get; set; }
        public string Subject1 { get; set; }
        public string Subject2 { get; set; }
        public int? AgeRangeFrom { get; set; }
        public int? AgeRangeTo { get; set; }
    }

    public class UpdateTeacherRequestRequestQualification
    {
        public string ProviderUkprn { get; set; }
        public string CountryCode { get; set; }
        public string Subject { get; set; }
        public ClassDivision? Class { get; set; }
        public DateOnly? Date { get; set; }
    }
}
