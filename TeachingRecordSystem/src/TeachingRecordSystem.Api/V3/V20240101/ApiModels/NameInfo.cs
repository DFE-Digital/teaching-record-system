namespace TeachingRecordSystem.Api.V3.V20240101.ApiModels;

public record NameInfo
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
}
