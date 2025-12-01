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
}
