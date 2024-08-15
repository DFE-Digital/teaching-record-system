namespace TeachingRecordSystem.Api.V3.V20240416.ApiModels;

[AutoMap(typeof(Core.SharedModels.NameInfo))]
public record NameInfo
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
}
