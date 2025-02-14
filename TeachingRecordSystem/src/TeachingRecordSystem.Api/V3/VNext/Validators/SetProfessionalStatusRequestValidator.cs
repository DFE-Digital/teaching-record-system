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
            .WithMessage($"QTS date cannot be in the future.");

        RuleFor(r => r.TrainingStartDate)
            .NotNull()
            .WithMessage("Training start date is required.");

        RuleFor(r => r.TrainingEndDate)
            .NotNull()
            .WithMessage("Training end date is required.");

        // 1. If early years and teacher already has EYTS date set then return an error
        // 2. If not early years and teacher already has QTS date set then return an error
        // 3. If existing ITT is for early years and trying to change route type to non early years then return an error
        // What status can overseas teachers have - is it just Approved or Withdrawn?
        // Can you even set Withdrawn in the API - doesn't look like it from existing code
    }
}
