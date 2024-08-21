using FluentValidation;
using TeachingRecordSystem.Api.V3.V20240814.Requests;

namespace TeachingRecordSystem.Api.V3.V20240814.Validators;

public class FindPersonsRequestValidator : AbstractValidator<FindPersonsRequest>
{
    public FindPersonsRequestValidator()
    {
        RuleFor(r => r.Persons.Count)
            .LessThanOrEqualTo(500)
            .WithMessage("Only 500 persons or less can be specified.")
            .OverridePropertyName("Persons");
    }
}