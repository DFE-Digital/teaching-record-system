using FluentValidation;
using TeachingRecordSystem.Api.V3.VNext.Requests;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using RouteToProfessionalStatusStatus = TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos.RouteToProfessionalStatusStatus;

namespace TeachingRecordSystem.Api.V3.VNext.Validators;

public class SetRouteToProfessionalStatusRequestValidator : AbstractValidator<SetRouteToProfessionalStatusRequest>
{
    public SetRouteToProfessionalStatusRequestValidator(IClock clock)
    {
        RuleFor(r => r.Status)
            .Cascade(CascadeMode.Stop)
            .IsInEnum()
            .Must(s => s != RouteToProfessionalStatusStatus.InTraining)
            .When(r => r.RouteToProfessionalStatusTypeId == PostgresModels.RouteToProfessionalStatusType.AssessmentOnlyRouteId, ApplyConditionTo.CurrentValidator)
            .WithMessage($"Status cannot be 'InTraining' when route type is '{PostgresModels.RouteToProfessionalStatusType.AssessmentOnlyRouteId}'.")
            .Must(s => s != RouteToProfessionalStatusStatus.UnderAssessment)
            .When(r => r.RouteToProfessionalStatusTypeId != PostgresModels.RouteToProfessionalStatusType.AssessmentOnlyRouteId, ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Status cannot be 'UnderAssessment' when route type is '{r.RouteToProfessionalStatusTypeId}'.");

        RuleFor(r => r.HoldsFrom)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .When(r => r.Status == RouteToProfessionalStatusStatus.Holds, ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Holds from date must be specified when status is '{r.Status}'.")
            .Null()
            .When(r => r.Status != RouteToProfessionalStatusStatus.Holds, ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Holds from date cannot be specified when status is '{r.Status}'.")
            .LessThanOrEqualTo(clock.Today)
            .WithMessage("Holds from date cannot be in the future.");

        RuleFor(r => r.TrainingStartDate)
            .NotNull()
            .WithMessage("Training start date must be specified.");

        RuleFor(r => r.TrainingEndDate)
            .NotNull()
            .WithMessage("Training end date must be specified.")
            .GreaterThan(r => r.TrainingStartDate)
            .WithMessage("Training end date cannot be before training start date.");

        When(r => r.TrainingSubjectReferences.HasValue, () =>
        {
            RuleFor(r => r.TrainingSubjectReferences.ValueOr(new string[] { }).Length)
            .InclusiveBetween(0, 3)
            .OverridePropertyName(nameof(SetRouteToProfessionalStatusRequest.TrainingSubjectReferences))
            .WithMessage("A maximum of 3 training subject references are allowed.");
        });

        RuleFor(r => r.TrainingAgeSpecialism).SetValidator(new SetRouteToProfessionalStatusRequestTrainingAgeSpecialismValidator());

        When(r => r.RouteToProfessionalStatusTypeId.IsOverseas(), () =>
        {
            var invalidOverseasCountries = new[]
            {
                "GB",
                "GB-ENG",
                "XF",
                "GB-WLS",
                "GB-CYM",
                "XI"
            };
            RuleFor(r => r.TrainingCountryReference)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(r => $"Training country reference must be specified when route type is '{r.RouteToProfessionalStatusTypeId}'.")
                .Must(c => !invalidOverseasCountries.Contains(c))
                .WithMessage(r => $"Training country reference cannot be '{r.TrainingCountryReference}' when route type is '{r.RouteToProfessionalStatusTypeId}'.")
                .Equal("GB-SCT")
                .When(r => r.RouteToProfessionalStatusTypeId == PostgresModels.RouteToProfessionalStatusType.ScotlandRId, ApplyConditionTo.CurrentValidator)
                .WithMessage($"Training country reference must be 'GB-SCT' when route type is '{PostgresModels.RouteToProfessionalStatusType.ScotlandRId}'.")
                .NotEqual("GB-SCT")
                .When(r => r.RouteToProfessionalStatusTypeId != PostgresModels.RouteToProfessionalStatusType.ScotlandRId, ApplyConditionTo.CurrentValidator)
                .WithMessage($"Training country reference cannot be 'GB-SCT' when route type is not '{PostgresModels.RouteToProfessionalStatusType.ScotlandRId}'.")
                .Equal("GB-NIR")
                .When(r => r.RouteToProfessionalStatusTypeId == PostgresModels.RouteToProfessionalStatusType.NiRId, ApplyConditionTo.CurrentValidator)
                .WithMessage($"Training country reference must be 'GB-NIR' when route type is '{PostgresModels.RouteToProfessionalStatusType.NiRId}'.")
                .NotEqual("GB-NIR")
                .When(r => r.RouteToProfessionalStatusTypeId != PostgresModels.RouteToProfessionalStatusType.NiRId, ApplyConditionTo.CurrentValidator)
                .WithMessage($"Training country reference cannot be 'GB-NIR' when route type is not '{PostgresModels.RouteToProfessionalStatusType.NiRId}'.");
        })
        .Otherwise(() =>
        {
            RuleFor(r => r.TrainingCountryReference)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(r => $"Training country reference must be specified when route type is '{r.RouteToProfessionalStatusTypeId}'.")
                .When(r => r.RouteToProfessionalStatusTypeId == PostgresModels.RouteToProfessionalStatusType.InternationalQualifiedTeacherStatusId, ApplyConditionTo.CurrentValidator)
                .Must(c => c == "GB-WLS" || c == "GB-CYM")
                .When(r => r.RouteToProfessionalStatusTypeId == PostgresModels.RouteToProfessionalStatusType.WelshRId, ApplyConditionTo.CurrentValidator)
                .WithMessage($"Training country reference must be 'GB-WLS' or 'GB-CYM' when route type is '{PostgresModels.RouteToProfessionalStatusType.WelshRId}'.")
                .Must(c => c != "GB-WLS" && c != "GB-CYM")
                .When(r => r.RouteToProfessionalStatusTypeId != PostgresModels.RouteToProfessionalStatusType.WelshRId, ApplyConditionTo.CurrentValidator)
                .WithMessage(r => $"Training country reference cannot be '{r.TrainingCountryReference}' when route type is not '{PostgresModels.RouteToProfessionalStatusType.WelshRId}'.")
                .Equal("GB")
                .When(r => r.RouteToProfessionalStatusTypeId != PostgresModels.RouteToProfessionalStatusType.InternationalQualifiedTeacherStatusId, ApplyConditionTo.CurrentValidator)
                .WithMessage(r => $"Training country reference must be 'GB' when route type is '{r.RouteToProfessionalStatusTypeId}'.");
        });

        RuleFor(r => r.TrainingProviderUkprn)
            .NotEmpty()
            .When(r => !r.RouteToProfessionalStatusTypeId.IsOverseas(), ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Training provider UKPRN must be specified when route type is '{r.RouteToProfessionalStatusTypeId}'.")
            .Empty()
            .When(r => r.RouteToProfessionalStatusTypeId.IsOverseas(), ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Training provider UKPRN cannot be specified when route type is '{r.RouteToProfessionalStatusTypeId}'.");

        RuleFor(r => r.IsExemptFromInduction)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .When(r => r.RouteToProfessionalStatusTypeId.CanBeExemptFromInduction(), ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Is exempt from induction must be specified when route type is '{r.RouteToProfessionalStatusTypeId}'.")
            .Null()
            .When(r => !r.RouteToProfessionalStatusTypeId.CanBeExemptFromInduction(), ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Is exempt from induction cannot be specified when route type is '{r.RouteToProfessionalStatusTypeId}'.");
    }
}

public class SetRouteToProfessionalStatusRequestTrainingAgeSpecialismValidator : AbstractValidator<SetRouteToProfessionalStatusRequestTrainingAgeSpecialism?>
{
    public SetRouteToProfessionalStatusRequestTrainingAgeSpecialismValidator()
    {
        When(r => r is not null, () =>
        {
            RuleFor(r => r!.Type)
                .IsInEnum()
                .WithMessage("Invalid training age specialism type.");

            When(r => r!.Type == Core.ApiSchema.V3.V20250425.Dtos.TrainingAgeSpecialismType.Range, () =>
            {
                RuleFor(r => r!.From)
                    .NotNull()
                    .WithMessage(s => $"From age must be specified for specialism type '{s!.Type}'.");

                RuleFor(r => r!.To)
                    .NotNull()
                    .WithMessage(s => $"To age must be specified for specialism type '{s!.Type}'.")
                    .GreaterThan(r => r!.From)
                    .WithMessage(s => $"To age cannot be less than From age for specialism type '{s!.Type}'.");

                RuleFor(r => r!.From)
                    .InclusiveBetween(0, 19)
                    .WithMessage(s => $"From age must be 0-19 inclusive for specialism type '{s!.Type}'.");

                RuleFor(r => r!.To)
                    .InclusiveBetween(0, 19)
                    .WithMessage(s => $"To age must be 0-19 inclusive for specialism type '{s!.Type}'.");
            })
            .Otherwise(() =>
            {
                RuleFor(r => r!.From)
                    .Null()
                    .WithMessage(r => $"From age cannot be specified for specialism type '{r!.Type}'.");

                RuleFor(r => r!.To)
                    .Null()
                    .WithMessage(r => $"To age cannot be specified for specialism type '{r!.Type}'.");
            });
        });
    }
}
