using FluentValidation;
using TeachingRecordSystem.Api.V3.V20240101.Requests;

namespace TeachingRecordSystem.Api.V3.V20240101.Validators;

public class CreateNameChangeRequestValidator : AbstractValidator<CreateNameChangeRequestRequest>
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
