using FluentValidation;
using TeachingRecordSystem.Api.V3.VNext.Requests;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using ProfessionalStatusStatus = TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos.ProfessionalStatusStatus;

namespace TeachingRecordSystem.Api.V3.VNext.Validators;

public class SetProfessionalStatusRequestValidator : AbstractValidator<SetProfessionalStatusRequest>
{
    public SetProfessionalStatusRequestValidator(IClock clock)
    {
        RuleFor(r => r.Status)
            .IsInEnum()
            .Must(s => s == ProfessionalStatusStatus.Approved)
            .When(r => r.RouteTypeId.IsOverseas(), ApplyConditionTo.CurrentValidator)
            .WithMessage("Overseas teachers can only have status set to Approved.");

        RuleFor(r => r.AwardedDate)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .When(r => r.Status == ProfessionalStatusStatus.Awarded || r.Status == ProfessionalStatusStatus.Approved, ApplyConditionTo.CurrentValidator)
            .WithMessage("Awarded date must be specified when status is Awarded or Approved.")
            .Null()
            .When(r => r.Status != ProfessionalStatusStatus.Awarded && r.Status != ProfessionalStatusStatus.Approved, ApplyConditionTo.CurrentValidator)
            .WithMessage("Awarded date cannot be specified unless status is Awarded or Approved.")
            .LessThanOrEqualTo(clock.Today)
            .WithMessage("Awarded date cannot be in the future.");

        RuleFor(r => r.TrainingStartDate)
            .NotNull()
            .WithMessage("Training start date is required.");

        RuleFor(r => r.TrainingEndDate)
            .NotNull()
            .WithMessage("Training end date is required.")
            .GreaterThan(r => r.TrainingStartDate)
            .WithMessage("Training end date cannot be before Training start date");

        When(r => r.TrainingSubjectReferences.HasValue, () =>
        {
            RuleFor(r => r.TrainingSubjectReferences.ValueOr(new string[] { }).Length)
            .ExclusiveBetween(1, 3)
            .OverridePropertyName(nameof(SetProfessionalStatusRequest.TrainingSubjectReferences))
            .WithMessage("A maximum of 3 training subject references are allowed.");
        });

        RuleFor(r => r.TrainingAgeSpecialism).SetValidator(new SetProfessionalStatusRequestTrainingAgeSpecialismValidator());

        RuleFor(r => r.TrainingCountryReference)
            .NotEmpty()
            .When(r => r.RouteTypeId.IsOverseas(), ApplyConditionTo.CurrentValidator)
            .WithMessage("Training country reference is required.")
            .Empty()
            .When(r => !r.RouteTypeId.IsOverseas(), ApplyConditionTo.CurrentValidator)
            .WithMessage("Training country reference should be empty.");

        When(r => r.RouteTypeId.IsOverseas(), () =>
        {
            RuleFor(r => r.TrainingCountryReference)
                .NotEmpty()
                .WithMessage("Training country reference is required.");

            RuleFor(r => r.TrainingCountryReference)
                .NotEqual("GB")
                .NotEqual("GB-ENG")
                .WithMessage(r => $"Training country reference cannot be 'GB' or 'GB-ENG' when RouteType is '{r.RouteTypeId}'.");

            RuleFor(r => r.TrainingCountryReference)
                .Equal("GB-SCT")
                .When(r => r.RouteTypeId == RouteToProfessionalStatus.ScotlandRId, ApplyConditionTo.CurrentValidator)
                .WithMessage("Training country reference must be 'GB-SCT' when RouteType is '52835B1F-1F2E-4665-ABC6-7FB1EF0A80BB'.")
                .NotEqual("GB-SCT")
                .When(r => r.RouteTypeId != RouteToProfessionalStatus.ScotlandRId, ApplyConditionTo.CurrentValidator)
                .WithMessage("Training country reference cannot be 'GB-SCT' when RouteType is not '52835B1F-1F2E-4665-ABC6-7FB1EF0A80BB'.");

            RuleFor(r => r.TrainingCountryReference)
                .Equal("GB-NIR")
                .When(r => r.RouteTypeId == RouteToProfessionalStatus.NiRId, ApplyConditionTo.CurrentValidator)
                .WithMessage("Training country reference must be 'GB-NIR' when RouteType is '3604EF30-8F11-4494-8B52-A2F9C5371E03'.")
                .NotEqual("GB-NIR")
                .When(r => r.RouteTypeId != RouteToProfessionalStatus.NiRId, ApplyConditionTo.CurrentValidator)
                .WithMessage("Training country reference cannot be 'GB-NIR' when RouteType is not '3604EF30-8F11-4494-8B52-A2F9C5371E03'.");
        })
        .Otherwise(() =>
        {
            RuleFor(r => r.TrainingCountryReference)
                .Empty()
                .WithMessage("Training country reference should be empty.");
        });

        RuleFor(r => r.TrainingProviderUkprn)
            .NotEmpty()
            .When(r => !r.RouteTypeId.IsOverseas())
            .WithMessage("Training provider UKPRN is required.");

        RuleFor(r => r.IsExemptFromInduction)
            .NotNull()
            .When(r => r.RouteTypeId == RouteToProfessionalStatus.ApplyforQtsId
                || r.RouteTypeId == RouteToProfessionalStatus.NiRId
                || r.RouteTypeId == RouteToProfessionalStatus.QtlsAndSetMembershipId
                || r.RouteTypeId == RouteToProfessionalStatus.ScotlandRId, ApplyConditionTo.CurrentValidator)
            .WithMessage("Is exempt from induction is required.")
            .Null()
            .When(r => r.RouteTypeId != RouteToProfessionalStatus.ApplyforQtsId
                && r.RouteTypeId != RouteToProfessionalStatus.NiRId
                && r.RouteTypeId != RouteToProfessionalStatus.QtlsAndSetMembershipId
                && r.RouteTypeId != RouteToProfessionalStatus.ScotlandRId, ApplyConditionTo.CurrentValidator)
            .WithMessage("Is exempt from induction should be empty.");

        RuleFor(r => r.IsExemptFromInduction)
            .Equal(true)
            .When(r => r.RouteTypeId == RouteToProfessionalStatus.QtlsAndSetMembershipId)
            .WithMessage("Is exempt from induction must be true.");
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

            When(r => r!.Type == Core.ApiSchema.V3.VNext.Dtos.TrainingAgeSpecialismType.Range, () =>
            {
                RuleFor(r => r!.From)
                    .NotNull()
                    .WithMessage("From age is required.");

                RuleFor(r => r!.To)
                    .NotNull()
                    .WithMessage("To age is required.")
                    .GreaterThan(r => r!.From)
                    .WithMessage("To age cannot be less than from age.");

                RuleFor(r => r!.From)
                    .InclusiveBetween(0, 19)
                    .WithMessage("Age must be 0-19 inclusive.");

                RuleFor(r => r!.To)
                    .InclusiveBetween(0, 19)
                    .WithMessage("Age must be 0-19 inclusive.");
            })
            .Otherwise(() =>
            {
                RuleFor(r => r!.From)
                    .Null()
                    .WithMessage(r => $"From age should be empty for specialism '{r!.Type}'.");

                RuleFor(r => r!.To)
                    .Null()
                    .WithMessage(r => $"To age should be empty for specialism '{r!.Type}'.");
            });
        });
    }
}
