using FluentValidation;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Validators;

public class CreateTrnRequestValidator : AbstractValidator<CreateTrnRequestBody>
{
    public CreateTrnRequestValidator(IClock clock)
    {
        RuleFor(r => r.RequestId)
            .Matches(TrnRequest.ValidRequestIdPattern)
            .WithMessage(Properties.StringResources.ErrorMessages_RequestIdCanOnlyContainCharactersDigitsUnderscoresAndDashes)
            .MaximumLength(TrnRequest.RequestIdMaxLength)
            .WithMessage(Properties.StringResources.ErrorMessages_RequestIdMustBe100CharactersOrFewer);

        RuleFor(r => r.Person.FirstName)
            .NotEmpty()
            .MaximumLength(AttributeConstraints.Contact.FirstNameMaxLength);

        RuleFor(r => r.Person.MiddleName)
            .MaximumLength(AttributeConstraints.Contact.MiddleNameMaxLength);

        RuleFor(r => r.Person.LastName)
            .NotEmpty()
            .MaximumLength(AttributeConstraints.Contact.LastNameMaxLength);

        RuleFor(r => r.Person.DateOfBirth)
            .NotEmpty()
            .Custom((value, ctx) =>
            {
                if (value >= DateOnly.FromDateTime(clock.UtcNow) || value < new DateOnly(1940, 1, 1))
                {
                    ctx.AddFailure(ctx.PropertyName, StringResources.ErrorMessages_BirthDateIsOutOfRange);
                }
            });

        RuleFor(r => r.Person.Email)
            .EmailAddress()
            .MaximumLength(AttributeConstraints.Contact.EMailAddress1MaxLength);

        RuleFor(r => r.Person.NationalInsuranceNumber)
            .MaximumLength(AttributeConstraints.Contact.NationalInsuranceNumber_MaxLength)
            .WithMessage(Properties.StringResources.ErrorMessages_NationalInsuranceNumberMustBe9CharactersOrLess);

    }
}
