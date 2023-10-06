namespace TeachingRecordSystem.SupportUi.Pages.Common;

public record AlertType
{
    public required Guid AlertTypeId { get; init; }    
    public required string Name { get; init; }
    public required string Value { get; init; }
}
