using FluentValidation;
using TeachingRecordSystem.Api.V3.VNext.Requests;

namespace TeachingRecordSystem.Api.V3.VNext.Validators;

public class SetQtlsRequestValidator : AbstractValidator<SetQtlsRequest>
{
    public SetQtlsRequestValidator(IClock clock)
    {
        RuleFor(x => x.Trn)
            .Matches(@"^\d{7}$")
            .WithMessage(Properties.StringResources.ErrorMessages_TRNMustBe7Digits);

        RuleFor(x => x.QTSDate)
            .LessThanOrEqualTo(clock.Today)
            .WithMessage($"QTLS Date cannot be in the future.");
    }
}
