using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QualifiedTeachersApi.V2.ApiModels;
using Swashbuckle.AspNetCore.Annotations;

namespace QualifiedTeachersApi.V2.Requests
{
    public class SetNpqQualificationRequest : IRequest
    {
        [Required]
        public DateOnly? CompletionDate { get; set; }

        [Required]
        [FromQuery(Name = "trn"), SwaggerParameter(Required = true, Description = "Trn")]
        public string Trn { get; set; }
        [Required]
        public QualificationType? QualificationType { get; set; }
    }
}
