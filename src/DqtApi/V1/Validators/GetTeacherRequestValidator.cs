using DqtApi.DataStore.Crm.Models;
using DqtApi.Validation;
using FluentValidation;

namespace DqtApi.V1.Validators
{
    public class GetTeacherRequestValidator : AbstractValidator<GetTeacherRequest>
    {
        public GetTeacherRequestValidator()
        {
            RuleFor(x => x.TRN)
                .Matches(@"^\d{7}$")
                .WithMessage(Properties.StringResources.ErrorMessages_TRNMustBe7Digits);

            RuleFor(x => x.BirthDate)
                .GreaterThanOrEqualTo(Constants.MinCrmDateTime)
                .WithMessage(Properties.StringResources.ErrorMessages_BirthDateIsOutOfRange);
        }
    }
}
