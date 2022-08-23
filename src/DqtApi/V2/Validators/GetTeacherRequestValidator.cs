using DqtApi.V2.Requests;
using DqtApi.Validation;
using FluentValidation;

namespace DqtApi.V2.Validators
{
    public class GetTeacherRequestValidator : AbstractValidator<GetTeacherRequest>
    {
        public GetTeacherRequestValidator()
        {
            RuleFor(x => x.Trn)
                .Matches(@"^\d{7}$")
                .WithMessage(Properties.StringResources.ErrorMessages_TRNMustBe7Digits);

            RuleFor(x => x.BirthDate)
                .NotEmpty()
                .Must(value => value is null || value.Value.ToDateTime() >= Constants.MinCrmDateTime)
                    .WithMessage(Properties.StringResources.ErrorMessages_BirthDateIsOutOfRange);
        }
    }
}
