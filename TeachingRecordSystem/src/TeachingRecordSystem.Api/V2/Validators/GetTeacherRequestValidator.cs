#nullable disable
using FluentValidation;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V2.Requests;

namespace TeachingRecordSystem.Api.V2.Validators;

public class GetTeacherRequestValidator : AbstractValidator<GetTeacherRequest>
{
    public GetTeacherRequestValidator()
    {
        RuleFor(x => x.Trn)
            .Matches(@"^\d{7}$")
            .WithMessage(StringResources.ErrorMessages_TRNMustBe7Digits);
    }
}
