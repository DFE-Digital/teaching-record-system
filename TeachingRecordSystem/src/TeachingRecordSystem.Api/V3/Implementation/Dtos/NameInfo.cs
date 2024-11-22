namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record NameInfo
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
}
