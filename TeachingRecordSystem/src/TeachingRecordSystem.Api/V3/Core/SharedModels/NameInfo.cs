namespace TeachingRecordSystem.Api.V3.Core.SharedModels;

public record NameInfo
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
}
