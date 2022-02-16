using DqtApi.DataStore.Crm.Models;
using DqtApi.DataStore.Sql.Models;
using DqtApi.V2.Requests;
using FluentValidation;

namespace DqtApi.V2.Validators
{
    public class GetOrCreateTrnRequestValidator : AbstractValidator<GetOrCreateTrnRequest>
    {
        public GetOrCreateTrnRequestValidator()
        {
            RuleFor(r => r.RequestId)
                .Matches(TrnRequest.ValidRequestIdPattern)
                    .WithMessage(Properties.StringResources.ErrorMessages_RequestIdCanOnlyContainCharactersDigitsUnderscoresAndDashes)
                .MaximumLength(TrnRequest.RequestIdMaxLength)
                    .WithMessage(Properties.StringResources.ErrorMessages_RequestIdMustBe100CharactersOrFewer);

            RuleFor(r => r.FirstName)
                .NotEmpty()
                .MaximumLength(AttributeConstraints.Contact.FirstNameMaxLength);

            RuleFor(r => r.MiddleName)
                .MaximumLength(AttributeConstraints.Contact.MiddleNameMaxLength);

            RuleFor(r => r.LastName)
                .NotEmpty()
                .MaximumLength(AttributeConstraints.Contact.LastNameMaxLength);

            RuleFor(r => r.BirthDate)
                .NotEmpty();

            RuleFor(r => r.EmailAddress)
                .EmailAddress()
                .MaximumLength(AttributeConstraints.Contact.EMailAddress1MaxLength);

            RuleFor(r => r.Address.AddressLine1)
                .MaximumLength(AttributeConstraints.Contact.Address1_Line1MaxLength);

            RuleFor(r => r.Address.AddressLine2)
                .MaximumLength(AttributeConstraints.Contact.Address1_Line2MaxLength);

            RuleFor(r => r.Address.AddressLine3)
                .MaximumLength(AttributeConstraints.Contact.Address1_Line3MaxLength);

            RuleFor(r => r.Address.City)
                .MaximumLength(AttributeConstraints.Contact.Address1_CityMaxLength);

            RuleFor(r => r.Address.Country)
                .MaximumLength(AttributeConstraints.Contact.Address1_CountryMaxLength);

            RuleFor(r => r.Address.PostalCode)
                .MaximumLength(AttributeConstraints.Contact.Address1_PostalCodeLength);

            RuleFor(r => r.GenderCode)
                .IsInEnum();

            RuleFor(r => r.InitialTeacherTraining.ProviderUkprn)
                .NotEmpty();

            RuleFor(r => r.InitialTeacherTraining.ProgrammeStartDate)
                .NotNull();

            RuleFor(r => r.InitialTeacherTraining.ProgrammeEndDate)
                .NotNull();

            RuleFor(r => r.InitialTeacherTraining.ProgrammeType)
                .NotNull()
                .IsInEnum();

            RuleFor(r => r.Qualification.ProviderUkprn)
                .NotEmpty();

            RuleFor(r => r.Qualification.Class)
                .IsInEnum();
        }
    }
}
