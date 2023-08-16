using FluentValidation;
using TeachingRecordSystem.Api.V3.Requests;

namespace TeachingRecordSystem.Api.V3.Validators;

public class CreateNameChangeRequestValidator : AbstractValidator<CreateNameChangeRequest>
{
    public CreateNameChangeRequestValidator()
    {
        RuleFor(r => r.FirstName)
            .NotEmpty();

        RuleFor(r => r.LastName)
            .NotEmpty();

        RuleFor(r => r.EvidenceFileName)
            .NotEmpty();

        RuleFor(r => r.EvidenceFileUrl)
            .NotEmpty();
    }
}
