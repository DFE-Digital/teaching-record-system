using FastEndpoints;
using FluentValidation;
using TeachingRecordSystem.Api.V3.Requests;

namespace TeachingRecordSystem.Api.V3.Validators;

public class FindTeachersRequestValidator : Validator<FindTeachersRequest>
{
    public FindTeachersRequestValidator()
    {
        RuleFor(r => r.FindBy)
            .IsInEnum()
            .WithMessage("Invalid matching policy.");

        RuleFor(r => r.DateOfBirth)
            .NotNull()
            .When(r => r.FindBy == FindTeachersFindBy.LastNameAndDateOfBirth)
            .WithMessage($"A value is required when findBy is '{nameof(FindTeachersFindBy.LastNameAndDateOfBirth)}'.");

        RuleFor(r => r.LastName)
            .NotEmpty()
            .When(r => r.FindBy == FindTeachersFindBy.LastNameAndDateOfBirth)
            .WithMessage($"A value is required when findBy is '{nameof(FindTeachersFindBy.LastNameAndDateOfBirth)}'.");
    }
}
