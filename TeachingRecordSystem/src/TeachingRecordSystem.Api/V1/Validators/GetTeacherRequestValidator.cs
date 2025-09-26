#nullable disable
using FluentValidation;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V1.Requests;
using TeachingRecordSystem.Api.Validation;

namespace TeachingRecordSystem.Api.V1.Validators;

public class GetTeacherRequestValidator : AbstractValidator<GetTeacherRequest>
{
    public GetTeacherRequestValidator()
    {
        RuleFor(x => x.Trn)
            .Matches(@"^\d{7}$")
            .WithMessage(StringResources.ErrorMessages_TRNMustBe7Digits);

        RuleFor(x => x.BirthDate)
            .GreaterThanOrEqualTo(Constants.MinCrmDateTime)
            .WithMessage(StringResources.ErrorMessages_BirthDateIsOutOfRange);
    }
}
