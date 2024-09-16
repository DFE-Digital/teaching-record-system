using FluentValidation;
using TeachingRecordSystem.Api.V3.VNext.Requests;

namespace TeachingRecordSystem.Api.V3.VNext.Validators;

public class SetDeceasedRequestValidator : AbstractValidator<SetDeceasedRequest>
{
    public SetDeceasedRequestValidator(IClock clock)
    {
        RuleFor(x => x.DateOfDeath)
            .LessThanOrEqualTo(clock.Today)
            .WithMessage($"Date cannot be in the future.");
    }
}
