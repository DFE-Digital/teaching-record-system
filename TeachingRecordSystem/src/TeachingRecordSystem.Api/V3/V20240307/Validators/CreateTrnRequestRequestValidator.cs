using FluentValidation;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V3.V20240307.Requests;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Api.V3.V20240307.Validators;

public class CreateTrnRequestRequestValidator : AbstractValidator<CreateTrnRequestRequest>
{
    public CreateTrnRequestRequestValidator(IClock clock)
    {
        RuleFor(r => r.RequestId)
            .Matches(TrnRequest.ValidRequestIdPattern)
            .WithMessage(StringResources.ErrorMessages_RequestIdCanOnlyContainCharactersDigitsUnderscoresAndDashes)
            .MaximumLength(TrnRequest.RequestIdMaxLength)
            .WithMessage(StringResources.ErrorMessages_RequestIdMustBe100CharactersOrFewer);

        RuleFor(r => r.Person.FirstName)
            .NotEmpty()
            .MaximumLength(Person.FirstNameMaxLength);

        RuleFor(r => r.Person.MiddleName)
            .MaximumLength(Person.MiddleNameMaxLength);

        RuleFor(r => r.Person.LastName)
            .NotEmpty()
            .MaximumLength(Person.LastNameMaxLength);

        RuleFor(r => r.Person.DateOfBirth)
            .NotEmpty()
            .Custom((value, ctx) =>
            {
                if (value >= DateOnly.FromDateTime(clock.UtcNow) || value < new DateOnly(1940, 1, 1))
                {
                    ctx.AddFailure(ctx.PropertyPath, StringResources.ErrorMessages_BirthDateIsOutOfRange);
                }
            });

        RuleFor(r => r.Person.Email)
            .EmailAddress()
            .MaximumLength(Person.EmailAddressMaxLength);

        RuleFor(r => r.Person.NationalInsuranceNumber)
            .Custom((value, ctx) =>
            {
                if (!string.IsNullOrEmpty(value) && !NationalInsuranceNumber.TryParse(value, out _))
                {
                    ctx.AddFailure(ctx.PropertyPath, StringResources.ErrorMessages_EnterNinoNumberInCorrectFormat);
                }
            });
    }
}
