using FluentValidation;
using TeachingRecordSystem.Api.V3.Requests;

namespace TeachingRecordSystem.Api.V3.Validators;

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
