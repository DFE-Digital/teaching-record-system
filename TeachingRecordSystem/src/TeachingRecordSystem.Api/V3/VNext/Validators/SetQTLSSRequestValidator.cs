using FluentValidation;
using TeachingRecordSystem.Api.V3.VNext.Requests;

namespace TeachingRecordSystem.Api.V3.VNext.Validators;

public class SetQTLSSRequestValidator : AbstractValidator<SetQTLSRequest>
{
    public SetQTLSSRequestValidator(IClock clock)
    {
        RuleFor(x => x.Trn)
            .Matches(@"^\d{7}$")
            .WithMessage(Properties.StringResources.ErrorMessages_TRNMustBe7Digits);

        RuleFor(x => x.AwardedDate)
            .LessThanOrEqualTo(clock.Today)
            .WithMessage($"Awarded date cannot be in the future.");
    }
}
