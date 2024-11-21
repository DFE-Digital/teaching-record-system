using FluentValidation;
using TeachingRecordSystem.Api.V3.VNext.Requests;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.VNext.Validators;

public class CreateTrnRequestRequestValidator : AbstractValidator<CreateTrnRequestRequest>
{
    public CreateTrnRequestRequestValidator(IClock clock)
    {
        RuleFor(r => r.Person.Address!.AddressLine1)
            .MaximumLength(AttributeConstraints.Contact.Address1_Line1MaxLength)
            .When(r => r.Person.Address != null);

        RuleFor(r => r.Person.Address!.AddressLine2)
            .MaximumLength(AttributeConstraints.Contact.Address1_Line2MaxLength)
            .When(r => r.Person.Address != null);

        RuleFor(r => r.Person.Address!.AddressLine3)
            .MaximumLength(AttributeConstraints.Contact.Address1_Line3MaxLength)
            .When(r => r.Person.Address != null);

        RuleFor(r => r.Person.Address!.City)
            .MaximumLength(AttributeConstraints.Contact.Address1_CityMaxLength)
            .When(r => r.Person.Address != null);

        RuleFor(r => r.Person.Address!.Country)
            .MaximumLength(AttributeConstraints.Contact.Address1_CountryMaxLength)
            .When(r => r.Person.Address != null);

        RuleFor(r => r.Person.Address!.PostalCode)
            .MaximumLength(AttributeConstraints.Contact.Address1_PostalCodeLength)
            .When(r => r.Person.Address != null);

        RuleFor(r => r.Person.GenderCode)
            .IsInEnum();
    }
}


