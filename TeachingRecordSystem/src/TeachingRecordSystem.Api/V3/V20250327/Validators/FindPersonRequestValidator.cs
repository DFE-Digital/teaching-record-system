using FluentValidation;
using TeachingRecordSystem.Api.V3.V20250327.Requests;

namespace TeachingRecordSystem.Api.V3.V20250327.Validators;

public class FindPersonRequestValidator : AbstractValidator<FindPersonRequest>
{
    public FindPersonRequestValidator()
    {
        RuleFor(r => r.FindBy)
            .IsInEnum()
            .WithMessage("Invalid matching policy.");

        RuleFor(r => r.DateOfBirth)
            .NotNull()
            .When(r => r.FindBy == FindPersonFindBy.LastNameAndDateOfBirth)
            .WithMessage($"A value is required when findBy is '{nameof(FindPersonFindBy.LastNameAndDateOfBirth)}'.");

        RuleFor(r => r.LastName)
            .NotEmpty()
            .When(r => r.FindBy == FindPersonFindBy.LastNameAndDateOfBirth)
            .WithMessage($"A value is required when findBy is '{nameof(FindPersonFindBy.LastNameAndDateOfBirth)}'.");
    }
}
