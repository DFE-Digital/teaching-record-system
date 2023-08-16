#nullable disable
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSwag.Examples;
using TeachingRecordSystem.Api.V2.ApiModels;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Requests;

public class SetIttOutcomeRequest : IRequest<SetIttOutcomeResponse>
{
    [FromRoute, Description("The TRN of the teacher to set ITT outcome for."), JsonIgnore]
    public string Trn { get; set; }

    [Required, Description("The UKPRN of the ITT provider.")]
    public string IttProviderUkprn { get; set; }

    [Required]
    public IttOutcome? Outcome { get; set; }

    [Description($"The assessment date for a {nameof(IttOutcome.Pass)} outcome.")]
    public DateOnly? AssessmentDate { get; set; }

    [Required, Description("DoB of teacher"), FromQuery(Name = "birthdate"), JsonIgnore]
    public DateOnly? BirthDate { get; set; }

    [Description("SlugId of Teacher"), FromQuery(Name = "slugid"), JsonIgnore]
    public string SlugId { get; set; }
}

public class SetQtsRequestExample : IExampleProvider<SetIttOutcomeRequest>
{
    public SetIttOutcomeRequest GetExample() => new()
    {
        Trn = "1234567",
        IttProviderUkprn = "1001234",
        Outcome = IttOutcome.Pass,
        AssessmentDate = new DateOnly(2021, 12, 22),
        SlugId = "2c408fff-de46-4a6e-bf5a-e77aa9d36961"
    };
}
