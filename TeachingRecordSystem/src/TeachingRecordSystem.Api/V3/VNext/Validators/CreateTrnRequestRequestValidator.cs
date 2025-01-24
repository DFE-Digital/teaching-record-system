using FluentValidation;
using TeachingRecordSystem.Api.V3.VNext.Requests;

namespace TeachingRecordSystem.Api.V3.VNext.Validators;

public class CreateTrnRequestRequestValidator : AbstractValidator<CreateTrnRequestRequest>
{
    public CreateTrnRequestRequestValidator()
    {
        RuleFor(r => r.Person.Gender)
            .IsInEnum();
    }
}


