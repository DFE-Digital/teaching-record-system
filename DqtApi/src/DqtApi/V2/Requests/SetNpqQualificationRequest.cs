using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DqtApi.V2.ApiModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace DqtApi.V2.Requests
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
