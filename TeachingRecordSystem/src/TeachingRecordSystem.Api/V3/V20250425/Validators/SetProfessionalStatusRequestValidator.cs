using FluentValidation;
using TeachingRecordSystem.Api.V3.V20250425.Requests;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using ProfessionalStatusStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos.ProfessionalStatusStatus;

namespace TeachingRecordSystem.Api.V3.V20250425.Validators;

public class SetProfessionalStatusRequestValidator : AbstractValidator<SetProfessionalStatusRequest>
{
    public SetProfessionalStatusRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(r => r.Status)
            .Cascade(CascadeMode.Stop)
            .IsInEnum()
            .Must(s => s == ProfessionalStatusStatus.Approved)
            .When(r => r.RouteTypeId.IsOverseas(), ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Status must be 'Approved' when route type is '{r.RouteTypeId}'.")
            .Must(s => s != ProfessionalStatusStatus.Approved)
            .When(r => !r.RouteTypeId.IsOverseas(), ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Status cannot be 'Approved' when route type is '{r.RouteTypeId}'.")
            .Must(s => s != ProfessionalStatusStatus.InTraining)
            .When(r => r.RouteTypeId == RouteToProfessionalStatusType.AssessmentOnlyRouteId, ApplyConditionTo.CurrentValidator)
            .WithMessage($"Status cannot be 'InTraining' when route type is '{RouteToProfessionalStatusType.AssessmentOnlyRouteId}'.")
            .Must(s => s != ProfessionalStatusStatus.UnderAssessment)
            .When(r => r.RouteTypeId != RouteToProfessionalStatusType.AssessmentOnlyRouteId, ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Status cannot be 'UnderAssessment' when route type is '{r.RouteTypeId}'.");

        RuleFor(r => r.AwardedDate)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .When(r => r.Status == ProfessionalStatusStatus.Awarded || r.Status == ProfessionalStatusStatus.Approved, ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Awarded date must be specified when status is '{r.Status}'.")
            .Null()
            .When(r => r.Status != ProfessionalStatusStatus.Awarded && r.Status != ProfessionalStatusStatus.Approved, ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Awarded date cannot be specified when status is '{r.Status}'.")
            .LessThanOrEqualTo(timeProvider.Today)
            .WithMessage("Awarded date cannot be in the future.");

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
            .OverridePropertyName(nameof(SetProfessionalStatusRequest.TrainingSubjectReferences))
            .WithMessage("A maximum of 3 training subject references are allowed.");
        });

        RuleFor(r => r.TrainingAgeSpecialism).SetValidator(new SetProfessionalStatusRequestTrainingAgeSpecialismValidator());

        When(r => r.RouteTypeId.IsOverseas(), () =>
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
                .WithMessage(r => $"Training country reference must be specified when route type is '{r.RouteTypeId}'.")
                .Must(c => !invalidOverseasCountries.Contains(c))
                .WithMessage(r => $"Training country reference cannot be '{r.TrainingCountryReference}' when route type is '{r.RouteTypeId}'.")
                .Equal("GB-SCT")
                .When(r => r.RouteTypeId == RouteToProfessionalStatusType.ScotlandRId, ApplyConditionTo.CurrentValidator)
                .WithMessage($"Training country reference must be 'GB-SCT' when route type is '{RouteToProfessionalStatusType.ScotlandRId}'.")
                .NotEqual("GB-SCT")
                .When(r => r.RouteTypeId != RouteToProfessionalStatusType.ScotlandRId, ApplyConditionTo.CurrentValidator)
                .WithMessage($"Training country reference cannot be 'GB-SCT' when route type is not '{RouteToProfessionalStatusType.ScotlandRId}'.")
                .Equal("GB-NIR")
                .When(r => r.RouteTypeId == RouteToProfessionalStatusType.NiRId, ApplyConditionTo.CurrentValidator)
                .WithMessage($"Training country reference must be 'GB-NIR' when route type is '{RouteToProfessionalStatusType.NiRId}'.")
                .NotEqual("GB-NIR")
                .When(r => r.RouteTypeId != RouteToProfessionalStatusType.NiRId, ApplyConditionTo.CurrentValidator)
                .WithMessage($"Training country reference cannot be 'GB-NIR' when route type is not '{RouteToProfessionalStatusType.NiRId}'.");
        })
        .Otherwise(() =>
        {
            RuleFor(r => r.TrainingCountryReference)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(r => $"Training country reference must be specified when route type is '{r.RouteTypeId}'.")
                .When(r => r.RouteTypeId == RouteToProfessionalStatusType.InternationalQualifiedTeacherStatusId, ApplyConditionTo.CurrentValidator)
                .Must(c => c == "GB-WLS" || c == "GB-CYM")
                .When(r => r.RouteTypeId == RouteToProfessionalStatusType.WelshRId, ApplyConditionTo.CurrentValidator)
                .WithMessage($"Training country reference must be 'GB-WLS' or 'GB-CYM' when route type is '{RouteToProfessionalStatusType.WelshRId}'.")
                .Must(c => c != "GB-WLS" && c != "GB-CYM")
                .When(r => r.RouteTypeId != RouteToProfessionalStatusType.WelshRId, ApplyConditionTo.CurrentValidator)
                .WithMessage(r => $"Training country reference cannot be '{r.TrainingCountryReference}' when route type is not '{RouteToProfessionalStatusType.WelshRId}'.")
                .Equal("GB")
                .When(r => r.RouteTypeId != RouteToProfessionalStatusType.InternationalQualifiedTeacherStatusId, ApplyConditionTo.CurrentValidator)
                .WithMessage(r => $"Training country reference must be 'GB' when route type is '{r.RouteTypeId}'.");
        });

        RuleFor(r => r.TrainingProviderUkprn)
            .NotEmpty()
            .When(r => !r.RouteTypeId.IsOverseas(), ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Training provider UKPRN must be specified when route type is '{r.RouteTypeId}'.")
            .Empty()
            .When(r => r.RouteTypeId.IsOverseas(), ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Training provider UKPRN cannot be specified when route type is '{r.RouteTypeId}'.");

        RuleFor(r => r.IsExemptFromInduction)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .When(r => r.RouteTypeId.CanBeExemptFromInduction(), ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Is exempt from induction must be specified when route type is '{r.RouteTypeId}'.")
            .Null()
            .When(r => !r.RouteTypeId.CanBeExemptFromInduction(), ApplyConditionTo.CurrentValidator)
            .WithMessage(r => $"Is exempt from induction cannot be specified when route type is '{r.RouteTypeId}'.");
    }
}

public class SetProfessionalStatusRequestTrainingAgeSpecialismValidator : AbstractValidator<SetProfessionalStatusRequestTrainingAgeSpecialism?>
{
    public SetProfessionalStatusRequestTrainingAgeSpecialismValidator()
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
