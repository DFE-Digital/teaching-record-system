using Optional;

namespace TeachingRecordSystem.Core.Services.Persons;

public record PersonDetailsToUpdate
{
    public Option<string>? FirstName { get; set; }
    public Option<string>? MiddleName { get; set; }
    public Option<string>? LastName { get; set; }
    public Option<DateOnly?>? DateOfBirth { get; set; }
    public Option<EmailAddress?>? EmailAddress { get; set; }
    public Option<NationalInsuranceNumber?>? NationalInsuranceNumber { get; set; }
    public Option<Gender?>? Gender { get; set; }

    public PersonDetails Resolve(PersonDetails oldPersonDetails) => new()
    {
        FirstName = (FirstName ?? Option.None<string>()).ValueOr(oldPersonDetails.FirstName),
        MiddleName = (MiddleName ?? Option.None<string>()).ValueOr(oldPersonDetails.MiddleName),
        LastName = (LastName ?? Option.None<string>()).ValueOr(oldPersonDetails.LastName),
        DateOfBirth = (DateOfBirth ?? Option.None<DateOnly?>()).ValueOr(oldPersonDetails.DateOfBirth),
        EmailAddress = (EmailAddress ?? Option.None<EmailAddress?>()).ValueOr(oldPersonDetails.EmailAddress),
        NationalInsuranceNumber = (NationalInsuranceNumber ?? Option.None<NationalInsuranceNumber?>()).ValueOr(oldPersonDetails.NationalInsuranceNumber),
        Gender = (Gender ?? Option.None<Gender?>()).ValueOr(oldPersonDetails.Gender),
    };
}
