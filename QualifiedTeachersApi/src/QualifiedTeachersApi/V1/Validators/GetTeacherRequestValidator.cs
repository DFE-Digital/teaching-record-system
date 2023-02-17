using FluentValidation;
using QualifiedTeachersApi.V1.Requests;
using QualifiedTeachersApi.Validation;

namespace QualifiedTeachersApi.V1.Validators
{
    public class GetTeacherRequestValidator : AbstractValidator<GetTeacherRequest>
    {
        public GetTeacherRequestValidator()
        {
            RuleFor(x => x.Trn)
                .Matches(@"^\d{7}$")
                .WithMessage(Properties.StringResources.ErrorMessages_TRNMustBe7Digits);

            RuleFor(x => x.BirthDate)
                .GreaterThanOrEqualTo(Constants.MinCrmDateTime)
                .WithMessage(Properties.StringResources.ErrorMessages_BirthDateIsOutOfRange);
        }
    }
}
