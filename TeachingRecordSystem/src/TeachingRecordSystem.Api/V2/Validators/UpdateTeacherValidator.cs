#nullable disable
using FluentValidation;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V2.ApiModels;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Dqt.Models;

namespace TeachingRecordSystem.Api.V2.Validators;

public class UpdateTeacherValidator : AbstractValidator<UpdateTeacherRequest>
{
    public UpdateTeacherValidator()
    {
        RuleFor(r => r.InitialTeacherTraining)
            .NotNull();

        RuleFor(r => r.InitialTeacherTraining.ProgrammeStartDate)
            .NotNull()
            .When(r => r.InitialTeacherTraining != null);

        RuleFor(r => r.InitialTeacherTraining.ProgrammeEndDate)
            .NotNull()
            .When(r => r.InitialTeacherTraining != null);

        RuleFor(r => r.InitialTeacherTraining.ProgrammeType)
            .NotNull()
            .IsInEnum()
            .When(r => r.InitialTeacherTraining != null);

        RuleFor(r => r.InitialTeacherTraining.AgeRangeFrom)
            .Must(v => AgeRange.TryConvertFromValue(v.Value, out _))
            .When(r => r.InitialTeacherTraining.AgeRangeFrom.HasValue)
            .WithMessage(StringResources.ErrorMessages_AgeMustBe0To19Inclusive)
            .When(r => r.InitialTeacherTraining != null);

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
            }).When(r => r.InitialTeacherTraining != null);

        RuleFor(r => r.InitialTeacherTraining.IttQualificationType)
            .IsInEnum()
            .Must(qt => qt != ApiModels.IttQualificationType.InternationalQualifiedTeacherStatus)
            .When(r => r.InitialTeacherTraining.ProgrammeType != ApiModels.IttProgrammeType.InternationalQualifiedTeacherStatus)
            .When(r => r.InitialTeacherTraining != null);


        RuleFor(r => new { r.InitialTeacherTraining.Outcome, r.InitialTeacherTraining.ProgrammeType })
            .Custom((request, ctx) =>
            {
                var validOutcomes = new[] { IttOutcome.UnderAssessment, IttOutcome.Deferred, IttOutcome.InTraining };
                if (!request.Outcome.HasValue)
                {
                    return;
                }

                if (!validOutcomes.Contains(request.Outcome.Value))
                {
                    ctx.AddFailure(nameof(request.Outcome), StringResources.ErrorMessages_OutcomeMustBeDeferredInTrainingOrUnderAssessment);
                }
                else
                {
                    if (request.ProgrammeType == IttProgrammeType.AssessmentOnlyRoute && request.Outcome.Value == IttOutcome.InTraining)
                    {
                        ctx.AddFailure(nameof(request.Outcome), StringResources.ErrorMessages_InTrainingOutcomeNotValidForAssessmentOnlyRoute);
                    }
                    else if (request.ProgrammeType != IttProgrammeType.AssessmentOnlyRoute && request.Outcome.Value == IttOutcome.UnderAssessment)
                    {
                        ctx.AddFailure(nameof(request.Outcome), StringResources.ErrorMessages_UnderAssessmentOutcomeOnlyValidForAssessmentOnlyRoute);
                    }
                }

            }).When(r => r.InitialTeacherTraining != null); ;

        RuleFor(r => r.Qualification.Class)
            .IsInEnum()
            .When(r => r.Qualification != null);

        RuleFor(r => r.InitialTeacherTraining.ProviderUkprn)
            .NotEmpty()
            .WithMessage("Initial TeacherTraining ProviderUkprn is required.")
            .When(r => r.InitialTeacherTraining != null); ;

        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .WithMessage("Birthdate is required.");

        RuleFor(r => r.Qualification.HeQualificationType)
            .IsInEnum()
            .When(r => r.Qualification != null);

        RuleFor(r => r.InitialTeacherTraining.IttQualificationAim)
            .IsInEnum()
            .When(r => r.InitialTeacherTraining != null); ;

        RuleFor(r => r.InitialTeacherTraining.TrainingCountryCode)
            .Empty()
            .When(r => r.InitialTeacherTraining.ProgrammeType != IttProgrammeType.InternationalQualifiedTeacherStatus, ApplyConditionTo.CurrentValidator)
            .When(r => r.InitialTeacherTraining != null, ApplyConditionTo.AllValidators)
            .When(r => r.InitialTeacherTraining != null);

        RuleFor(r => r.SlugId)
            .MaximumLength(AttributeConstraints.Contact.SlugId_MaxLength)
            .WithMessage(Properties.StringResources.ErrorMessages_SlugIdMustBe150CharactersOrFewer);
    }
}
