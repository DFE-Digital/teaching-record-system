using FluentValidation;
using QualifiedTeachersApi.V2.Requests;

namespace QualifiedTeachersApi.V2.Validators;

public class GetTrnDetailsRequestValidator : AbstractValidator<FindTeachersRequest>
{
    public GetTrnDetailsRequestValidator()
    {
        RuleFor(m => m.IttProviderName).Empty().When(m => !string.IsNullOrEmpty(m.IttProviderUkprn))
            .WithMessage(Properties.StringResources.ErrorMessage_EitherIttProviderNameOrIttProviderUkprn);
        RuleFor(m => m.IttProviderUkprn).Empty().When(m => !string.IsNullOrEmpty(m.IttProviderName))
            .WithMessage(Properties.StringResources.ErrorMessage_EitherIttProviderNameOrIttProviderUkprn);
    }
}
