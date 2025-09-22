using FluentValidation;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V3.V20250203.Requests;

namespace TeachingRecordSystem.Api.V3.V20250203.Validators;

public class CreateTrnRequestRequestValidator : AbstractValidator<CreateTrnRequestRequest>
{
    public CreateTrnRequestRequestValidator(IClock clock)
    {
        RuleFor(r => r.RequestId)
            .Matches(PostgresModels.TrnRequest.ValidRequestIdPattern)
            .WithMessage(StringResources.ErrorMessages_RequestIdCanOnlyContainCharactersDigitsUnderscoresAndDashes)
            .MaximumLength(PostgresModels.TrnRequest.RequestIdMaxLength)
            .WithMessage(StringResources.ErrorMessages_RequestIdMustBe100CharactersOrFewer);

        RuleFor(r => r.Person.FirstName)
            .NotEmpty()
            .MaximumLength(PostgresModels.Person.FirstNameMaxLength);

        RuleFor(r => r.Person.MiddleName)
            .MaximumLength(PostgresModels.Person.MiddleNameMaxLength);

        RuleFor(r => r.Person.LastName)
            .NotEmpty()
            .MaximumLength(PostgresModels.Person.LastNameMaxLength);

        RuleFor(r => r.Person.DateOfBirth)
            .NotEmpty()
            .Custom((value, ctx) =>
            {
                if (value >= DateOnly.FromDateTime(clock.UtcNow) || value < new DateOnly(1940, 1, 1))
                {
                    ctx.AddFailure(ctx.PropertyName, StringResources.ErrorMessages_BirthDateIsOutOfRange);
                }
            });

        RuleForEach(r => r.Person.EmailAddresses)
            .NotNull()
            .WithMessage("Email address cannot be null.")
            .EmailAddress()
            .MaximumLength(PostgresModels.Person.EmailAddressMaxLength);

        RuleFor(r => r.Person.NationalInsuranceNumber)
            .Custom((value, ctx) =>
            {
                if (!string.IsNullOrEmpty(value) && !NationalInsuranceNumber.TryParse(value, out _))
                {
                    ctx.AddFailure(ctx.PropertyName, StringResources.ErrorMessages_EnterNinoNumberInCorrectFormat);
                }
            });

        RuleFor(r => r.Person.Gender)
            .IsInEnum();
    }
}


