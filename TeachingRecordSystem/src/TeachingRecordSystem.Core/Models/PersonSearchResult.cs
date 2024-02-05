namespace TeachingRecordSystem.Core.Models;

public class PersonSearchResult
{
    public required Guid PersonId { get; set; }
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string LastName { get; set; }
    public required DateOnly? DateOfBirth { get; set; }
    public required string? Trn { get; set; }
    public string? NationalInsuranceNumber { get; set; }
}
