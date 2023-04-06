using FluentValidation;
using QualifiedTeachersApi.V3.Requests;

namespace QualifiedTeachersApi.V3.Validators;

public class CreateDateOfBirthChangeRequestValidator : AbstractValidator<CreateDateOfBirthChangeRequest>
{
    public CreateDateOfBirthChangeRequestValidator()
    {
        RuleFor(r => r.DateOfBirth)
            .NotNull();

        RuleFor(r => r.EvidenceFileName)
            .NotEmpty();

        RuleFor(r => r.EvidenceFileUrl)
            .NotEmpty();
    }
}
