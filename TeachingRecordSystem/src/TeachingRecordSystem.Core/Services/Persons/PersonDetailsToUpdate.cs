using Optional;

namespace TeachingRecordSystem.Core.Services.Persons;

public record PersonDetailsToUpdate
{
    public required Option<string> FirstName { get; set; }
    public required Option<string> MiddleName { get; set; }
    public required Option<string> LastName { get; set; }
    public required Option<DateOnly?> DateOfBirth { get; set; }
    public required Option<EmailAddress?> EmailAddress { get; set; }
    public required Option<NationalInsuranceNumber?> NationalInsuranceNumber { get; set; }
    public required Option<Gender?> Gender { get; set; }

    public PersonDetails Resolve(PersonDetails oldPersonDetails) => new()
    {
        FirstName = FirstName.ValueOr(oldPersonDetails.FirstName),
        MiddleName = MiddleName.ValueOr(oldPersonDetails.MiddleName),
        LastName = LastName.ValueOr(oldPersonDetails.LastName),
        DateOfBirth = DateOfBirth.ValueOr(oldPersonDetails.DateOfBirth),
        EmailAddress = EmailAddress.ValueOr(oldPersonDetails.EmailAddress),
        NationalInsuranceNumber = NationalInsuranceNumber.ValueOr(oldPersonDetails.NationalInsuranceNumber),
        Gender = Gender.ValueOr(oldPersonDetails.Gender),
    };
}
