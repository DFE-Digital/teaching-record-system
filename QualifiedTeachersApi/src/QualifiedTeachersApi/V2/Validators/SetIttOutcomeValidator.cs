#nullable disable
using FluentValidation;
using QualifiedTeachersApi.V2.Requests;

namespace QualifiedTeachersApi.V2.Validators;

public class SetIttOutcomeValidator : AbstractValidator<SetIttOutcomeRequest>
{
    public SetIttOutcomeValidator(IClock clock)
    {
        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .WithMessage("Birthdate is required.");

        RuleFor(c => c.IttProviderUkprn)
            .NotEmpty()
            .WithMessage("ITT provider UKPRN is required.");

        RuleFor(c => c.AssessmentDate)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .When(c => c.Outcome == ApiModels.IttOutcome.Pass, ApplyConditionTo.CurrentValidator)
            .WithMessage("Assessment date must be specified when outcome is Pass.")
            .Null()
            .When(c => c.Outcome != ApiModels.IttOutcome.Pass, ApplyConditionTo.CurrentValidator)
            .WithMessage("Assessment date cannot be specified unless outcome is Pass.")
            .LessThanOrEqualTo(clock.Today)
            .WithMessage($"QTS date cannot be in the future.");

        RuleFor(c => c.Outcome)
            .NotNull()
            .IsInEnum();
    }
}
