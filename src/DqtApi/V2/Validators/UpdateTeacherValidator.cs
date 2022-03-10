using DqtApi.DataStore.Crm.Models;
using DqtApi.Properties;
using DqtApi.V2.Requests;
using FluentValidation;

namespace DqtApi.V2.Validators
{
    public class UpdateTeacherValidator : AbstractValidator<UpdateTeacherRequest>
    {
        public UpdateTeacherValidator()
        {
            RuleFor(r => r.InitialTeacherTraining.ProgrammeStartDate)
              .NotNull();

            RuleFor(r => r.InitialTeacherTraining.ProgrammeEndDate)
                .NotNull();

            RuleFor(r => r.InitialTeacherTraining.ProgrammeType)
                .NotNull()
                .IsInEnum();

            RuleFor(r => r.InitialTeacherTraining.AgeRangeFrom)
              .Must(v => AgeRange.TryConvertFromValue(v.Value, out _))
                  .When(r => r.InitialTeacherTraining.AgeRangeFrom.HasValue)
                  .WithMessage(StringResources.ErrorMessages_AgeMustBe0To19Inclusive);

            RuleFor(r => r.InitialTeacherTraining.AgeRangeTo)
                .Custom((value, ctx) =>
                {
                    if (!value.HasValue)
                    {
                        return;
                    }

                    if (!AgeRange.TryConvertFromValue(value.Value, out var ageTo))
                    {
                        ctx.AddFailure(ctx.PropertyName, StringResources.ErrorMessages_AgeMustBe0To19Inclusive);
                        return;
                    }

                    if (ctx.InstanceToValidate.InitialTeacherTraining.AgeRangeFrom.HasValue &&
                        ctx.InstanceToValidate.InitialTeacherTraining.AgeRangeFrom.Value > value)
                    {
                        ctx.AddFailure(ctx.PropertyName, StringResources.ErrorMessages_AgeToCannotBeLessThanAgeFrom);
                    }
                });


            RuleFor(r => r.Qualification.ProviderUkprn)
                .NotEmpty();

            RuleFor(r => r.Qualification.Class)
                .IsInEnum();

            RuleFor(x => x.BirthDate)
                .NotEmpty()
                .WithMessage("Birthdate is required.");
        }
    }
}
