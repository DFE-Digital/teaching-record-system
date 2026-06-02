namespace TeachingRecordSystem.Core.Services.OneLogin;

public record GetMatchedAttributesOptions
{
    public required Guid PersonId { get; init; }
    public required IEnumerable<string[]> Names { get; init; }
    public required IEnumerable<DateOnly> DatesOfBirth { get; init; }
    public required string? EmailAddress { get; init; }
}
