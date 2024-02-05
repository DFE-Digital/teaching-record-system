namespace TeachingRecordSystem.Api.V3.Requests;

public record TrnRequestPerson
{
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public required DateOnly DateOfBirth { get; set; }
    public string? Email { get; set; }
    public string? NationalInsuranceNumber { get; set; }
}
