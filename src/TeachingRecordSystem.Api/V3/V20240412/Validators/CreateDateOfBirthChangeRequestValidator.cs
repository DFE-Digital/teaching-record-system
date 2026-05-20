using FluentValidation;
using TeachingRecordSystem.Api.V3.V20240412.Requests;

namespace TeachingRecordSystem.Api.V3.V20240412.Validators;

public class CreateDateOfBirthChangeRequestValidator : AbstractValidator<CreateDateOfBirthChangeRequestRequest>
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
