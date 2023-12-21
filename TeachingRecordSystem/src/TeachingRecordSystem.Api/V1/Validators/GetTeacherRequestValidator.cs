#nullable disable
using FluentValidation;
using TeachingRecordSystem.Api.V1.Requests;

namespace TeachingRecordSystem.Api.V1.Validators;

public class GetTeacherRequestValidator : AbstractValidator<GetTeacherRequest>
{
    public GetTeacherRequestValidator()
    {
        RuleFor(x => x.Trn)
            .Matches(@"^\d{7}$")
            .WithMessage(Properties.StringResources.ErrorMessages_TRNMustBe7Digits);

        RuleFor(x => x.BirthDate)
            .GreaterThanOrEqualTo(Validation.Constants.MinCrmDateTime)
            .WithMessage(Properties.StringResources.ErrorMessages_BirthDateIsOutOfRange);
    }
}
