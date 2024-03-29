#nullable disable
using FluentValidation;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V2.Requests;

namespace TeachingRecordSystem.Api.V2.Validators;

public class SetNpqQualificationValidator : AbstractValidator<SetNpqQualificationRequest>
{
    public SetNpqQualificationValidator(IClock clock)
    {
        RuleFor(x => x.Trn).NotNull();

        RuleFor(r => r.QualificationType)
            .NotNull()
            .IsInEnum();

        RuleFor(r => r.CompletionDate)
            .Custom((value, ctx) =>
            {
                if (!value.HasValue)
                {
                    return;
                }

                if (ctx.InstanceToValidate.CompletionDate.Value > clock.Today)
                {
                    ctx.AddFailure(ctx.PropertyName, StringResources.Errors_10019_Title);
                }
                else if (ctx.InstanceToValidate.CompletionDate.Value < new DateOnly(2021, 11, 1))
                {
                    ctx.AddFailure(ctx.PropertyName, StringResources.Errors_10022_Title);
                }
            });
    }
}
