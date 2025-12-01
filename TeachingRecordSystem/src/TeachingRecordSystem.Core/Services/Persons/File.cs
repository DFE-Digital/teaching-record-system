namespace TeachingRecordSystem.Core.Services.Persons;

public record File
{
    public required Guid FileId { get; init; }
    public required string Name { get; init; }

    public EventModels.File ToEventModel() => new()
    {
        FileId = FileId,
        Name = Name
    };
}
