using FluentValidation;
using TeachingRecordSystem.Api.V3.V20240912.Requests;

namespace TeachingRecordSystem.Api.V3.V20240920.Validators;

public class SetQtlsRequestValidator : AbstractValidator<SetQtlsRequest>
{
    public SetQtlsRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.QtsDate)
            .LessThanOrEqualTo(timeProvider.Today)
            .WithMessage($"Date cannot be in the future.");
    }
}
