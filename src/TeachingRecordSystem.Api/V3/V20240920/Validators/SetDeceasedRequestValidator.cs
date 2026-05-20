using FluentValidation;
using TeachingRecordSystem.Api.V3.V20240920.Requests;

namespace TeachingRecordSystem.Api.V3.V20240920.Validators;

public class SetDeceasedRequestValidator : AbstractValidator<SetDeceasedRequest>
{
    public SetDeceasedRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.DateOfDeath)
            .LessThanOrEqualTo(timeProvider.Today)
            .WithMessage($"Date cannot be in the future.");
    }
}
