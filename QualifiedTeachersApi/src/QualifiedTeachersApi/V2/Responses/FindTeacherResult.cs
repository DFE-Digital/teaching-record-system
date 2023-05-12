namespace QualifiedTeachersApi.V2.Responses;

public record FindTeacherResult
{
    public required string Trn { get; set; }
    public required string[] EmailAddresses { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required DateOnly? DateOfBirth { get; set; }
    public required string? NationalInsuranceNumber { get; set; }
    public required string Uid { get; set; }
    public required bool HasActiveSanctions { get; set; }
}
