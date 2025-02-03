using FluentValidation;
using TeachingRecordSystem.Api.V3.V20250203.Requests;

namespace TeachingRecordSystem.Api.V3.V20250203.Validators;

public class CreateTrnRequestRequestValidator : AbstractValidator<CreateTrnRequestRequest>
{
    public CreateTrnRequestRequestValidator()
    {
        RuleFor(r => r.Person.Gender)
            .IsInEnum();
    }
}


