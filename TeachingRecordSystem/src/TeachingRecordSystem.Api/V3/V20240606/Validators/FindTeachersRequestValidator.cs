using FluentValidation;
using TeachingRecordSystem.Api.V3.V20240606.Requests;

namespace TeachingRecordSystem.Api.V3.V20240606.Validators;

public class FindTeachersRequestValidator : AbstractValidator<FindPersonRequest>
{
    public FindTeachersRequestValidator()
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
