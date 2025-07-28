using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Events.Models;

public record PersonDetails
{
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string LastName { get; set; }
    public required DateOnly? DateOfBirth { get; set; }
    public required string? EmailAddress { get; set; }
    public required string? NationalInsuranceNumber { get; set; }
    public required Gender? Gender { get; set; }

    public static PersonDetails FromModel(Person person) => new()
    {
        FirstName = person.FirstName,
        MiddleName = person.MiddleName,
        LastName = person.LastName,
        DateOfBirth = person.DateOfBirth,
        EmailAddress = person.EmailAddress,
        NationalInsuranceNumber = person.NationalInsuranceNumber,
        Gender = person.Gender
    };
}
