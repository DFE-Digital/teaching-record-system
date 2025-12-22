namespace TeachingRecordSystem.Core.Services.Persons;

public record Justification<TReason>
{
    public required TReason Reason { get; init; }
    public string? ReasonDetail { get; init; }
    public File? Evidence { get; init; }
}
