namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

public record PersonInfo
{
    public required string Trn { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
}
