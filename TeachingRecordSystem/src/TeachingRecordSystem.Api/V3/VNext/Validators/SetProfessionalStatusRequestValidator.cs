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

        RuleFor(r => r.TrainingAgeSpecialism).SetValidator(new SetProfessionalStatusRequestTrainingAgeSpecialismValidator());

        RuleFor(r => r.TrainingCountryReference)
            .NotEmpty()
            .When(r => r.RouteTypeId.IsOverseas(), ApplyConditionTo.CurrentValidator)
            .WithMessage("Training country reference is required.")
            .Empty()
            .When(r => !r.RouteTypeId.IsOverseas(), ApplyConditionTo.CurrentValidator)
            .WithMessage("Training country reference should be empty.");

        RuleFor(r => r.TrainingCountryReference)
            .Custom((countryCode, ctx) =>
            {
                if (ctx.InstanceToValidate.RouteTypeId.IsOverseas() == false)
                {
                    return;
                }

                var routeId = ctx.InstanceToValidate.RouteTypeId;
                if (countryCode == "GB" || countryCode == "GB-ENG")
                {
                    ctx.AddFailure(
                        "TrainingCountryReference",
                        $"TrainingCountryReference cannot be 'GB' or 'GB-ENG' when RouteType is '{routeId}'.");
                    return;
                }

                if (routeId == Guid.Parse("52835B1F-1F2E-4665-ABC6-7FB1EF0A80BB"))
                {
                    if (countryCode != "GB-SCT")
                    {
                        ctx.AddFailure(
                            "TrainingCountryReference",
                            $"TrainingCountryReference must be 'GB-SCT' when RouteType is '{routeId}'.");
                    }
                }
                else if (routeId == Guid.Parse("3604EF30-8F11-4494-8B52-A2F9C5371E03"))
                {
                    if (countryCode != "GB-NIR")
                    {
                        ctx.AddFailure(
                            "TrainingCountryReference",
                            $"TrainingCountryReference must be 'GB-NIR' when RouteType is '{routeId}'.");
                    }
                }
                else
                {
                    if (countryCode == "GB-SCT" || countryCode == "GB-NIR")
                    {
                        ctx.AddFailure(
                            "TrainingCountryReference",
                            $"TrainingCountryReference cannot be 'GB' or 'GB-ENG' when RouteType is '{routeId}'.");
                    }
                }
            });

        RuleFor(r => r.TrainingProviderUkprn)
            .NotEmpty()
            .When(r => !r.RouteTypeId.IsOverseas())
            .WithMessage("Training provider UKPRN is required.");
    }
}

public class SetProfessionalStatusRequestTrainingAgeSpecialismValidator : AbstractValidator<SetProfessionalStatusRequestTrainingAgeSpecialism?>
{
    public SetProfessionalStatusRequestTrainingAgeSpecialismValidator()
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


        });
    }
}
