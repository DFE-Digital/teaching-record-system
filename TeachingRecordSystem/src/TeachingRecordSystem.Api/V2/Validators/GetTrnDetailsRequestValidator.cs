#nullable disable
using FluentValidation;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V2.Requests;

namespace TeachingRecordSystem.Api.V2.Validators;

public class GetTrnDetailsRequestValidator : AbstractValidator<FindTeachersRequest>
{
    public GetTrnDetailsRequestValidator()
    {
        RuleFor(m => m.IttProviderName).Empty().When(m => !string.IsNullOrEmpty(m.IttProviderUkprn))
            .WithMessage(StringResources.ErrorMessage_EitherIttProviderNameOrIttProviderUkprn);
        RuleFor(m => m.IttProviderUkprn).Empty().When(m => !string.IsNullOrEmpty(m.IttProviderName))
            .WithMessage(StringResources.ErrorMessage_EitherIttProviderNameOrIttProviderUkprn);
    }
}
