namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;

public record NameInfo
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
}
