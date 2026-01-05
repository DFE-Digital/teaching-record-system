using Optional;

namespace TeachingRecordSystem.Core.Services.Persons;

public record PersonDetails
{
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string LastName { get; set; }
    public required DateOnly? DateOfBirth { get; set; }
    public required EmailAddress? EmailAddress { get; set; }
    public required NationalInsuranceNumber? NationalInsuranceNumber { get; set; }
    public required Gender? Gender { get; set; }

    public EventModels.PersonDetails ToEventModel() => new()
    {
        FirstName = FirstName,
        MiddleName = MiddleName,
        LastName = LastName,
        DateOfBirth = DateOfBirth,
        EmailAddress = EmailAddress?.ToString(),
        NationalInsuranceNumber = NationalInsuranceNumber?.ToString(),
        Gender = Gender,
    };

    public PersonDetailsToUpdate UpdateAll()
    {
        return new()
        {
            FirstName = Option.Some(FirstName),
            MiddleName = Option.Some(MiddleName),
            LastName = Option.Some(LastName),
            DateOfBirth = Option.Some(DateOfBirth),
            EmailAddress = Option.Some(EmailAddress),
            NationalInsuranceNumber = Option.Some(NationalInsuranceNumber),
            Gender = Option.Some(Gender)
        };
    }

    public PersonDetailsToUpdate UpdateFromAttributes(IReadOnlyCollection<PersonMatchedAttribute> attributesToUpdate)
    {
        return new()
        {
            FirstName = attributesToUpdate.Contains(PersonMatchedAttribute.FirstName)
                ? Option.Some(FirstName!)
                : Option.None<string>(),
            MiddleName = attributesToUpdate.Contains(PersonMatchedAttribute.MiddleName)
                ? Option.Some(MiddleName ?? string.Empty)
                : Option.None<string>(),
            LastName = attributesToUpdate.Contains(PersonMatchedAttribute.LastName)
                ? Option.Some(LastName!)
                : Option.None<string>(),
            DateOfBirth = attributesToUpdate.Contains(PersonMatchedAttribute.DateOfBirth)
                ? Option.Some<DateOnly?>(DateOfBirth)
                : Option.None<DateOnly?>(),
            EmailAddress = attributesToUpdate.Contains(PersonMatchedAttribute.EmailAddress)
                ? Option.Some(EmailAddress)
                : Option.None<EmailAddress?>(),
            NationalInsuranceNumber = attributesToUpdate.Contains(PersonMatchedAttribute.NationalInsuranceNumber)
                ? Option.Some(NationalInsuranceNumber)
                : Option.None<NationalInsuranceNumber?>(),
            Gender = attributesToUpdate.Contains(PersonMatchedAttribute.Gender)
                ? Option.Some(Gender)
                : Option.None<Gender?>(),
        };
    }

}
