using DqtApi.V2.Requests;
using FluentValidation;

namespace DqtApi.V2.Validators
{
    public class SetTeacherIdentityInfoRequestValidator : AbstractValidator<SetTeacherIdentityInfoRequest>
    {
        public SetTeacherIdentityInfoRequestValidator()
        {
            RuleFor(r => r.TsPersonId)
                .NotEmpty()
                .WithMessage("TSPersonId is required.");
        }
    }
}
