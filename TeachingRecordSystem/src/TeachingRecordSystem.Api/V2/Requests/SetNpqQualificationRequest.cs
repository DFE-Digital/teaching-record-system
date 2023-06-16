#nullable disable
using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TeachingRecordSystem.Api.V2.ApiModels;

namespace TeachingRecordSystem.Api.V2.Requests;

public class SetNpqQualificationRequest : IRequest
{
    [Required]
    public DateOnly? CompletionDate { get; set; }

    [Required]
    [FromQuery(Name = "trn")]
    public string Trn { get; set; }

    [Required]
    public QualificationType? QualificationType { get; set; }
}
