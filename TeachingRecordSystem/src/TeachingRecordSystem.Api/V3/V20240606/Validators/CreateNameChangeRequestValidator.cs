using FluentValidation;
using TeachingRecordSystem.Api.V3.V20240606.Requests;

namespace TeachingRecordSystem.Api.V3.V20240606.Validators;

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
