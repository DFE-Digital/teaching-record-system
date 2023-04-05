#nullable disable
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QualifiedTeachersApi.V2.ApiModels;
using QualifiedTeachersApi.V2.Responses;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace QualifiedTeachersApi.V2.Requests;

public class SetIttOutcomeRequest : IRequest<SetIttOutcomeResponse>
{
    [FromRoute]
    [SwaggerParameter(description: "The TRN of the teacher to set ITT outcome for.")]
    [JsonIgnore]
    public string Trn { get; set; }

    [Required]
    [SwaggerParameter(description: "The UKPRN of the ITT provider.")]
    public string IttProviderUkprn { get; set; }

    [Required]
    public IttOutcome? Outcome { get; set; }

    [SwaggerParameter(description: $"The assessment date for a {nameof(IttOutcome.Pass)} outcome.")]
    public DateOnly? AssessmentDate { get; set; }

    [Required]
    [FromQuery(Name = "birthdate"), SwaggerParameter(Required = true, Description = "DoB of teacher"), SwaggerSchema(Format = "date"), ModelBinder(typeof(ModelBinding.DateModelBinder))]
    [JsonIgnore]
    public DateOnly? BirthDate { get; set; }
}

public class SetQtsRequestExample : IExamplesProvider<SetIttOutcomeRequest>
{
    public SetIttOutcomeRequest GetExamples() => new()
    {
        Trn = "1234567",
        IttProviderUkprn = "1001234",
        Outcome = IttOutcome.Pass,
        AssessmentDate = new DateOnly(2021, 12, 22)
    };
}
