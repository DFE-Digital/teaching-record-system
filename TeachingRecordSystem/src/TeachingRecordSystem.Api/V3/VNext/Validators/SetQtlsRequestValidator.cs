using FluentValidation;
using TeachingRecordSystem.Api.V3.VNext.Requests;

namespace TeachingRecordSystem.Api.V3.VNext.Validators;

public class SetQtlsRequestValidator : AbstractValidator<SetQtlsRequest>
{
    public SetQtlsRequestValidator(IClock clock)
    {
        RuleFor(x => x.QtsDate)
            .LessThanOrEqualTo(clock.Today)
            .WithMessage($"Date cannot be in the future.");
    }
}
