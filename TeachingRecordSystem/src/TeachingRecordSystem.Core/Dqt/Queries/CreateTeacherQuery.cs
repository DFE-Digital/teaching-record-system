namespace TeachingRecordSystem.Core.Dqt.Queries;

public class CreateContactQuery : ICrmQuery<Guid>
{
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? Email { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required FindingExistingTeachersResult[] ExistingTeacherResults { get; init; }
    public required string? Trn { get; init; }
}
