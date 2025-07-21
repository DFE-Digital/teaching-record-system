namespace TeachingRecordSystem.Core.Events;

public record PersonDetailsUpdatedFromTrnRequestEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required EventModels.PersonDetails Details { get; init; }
    public required EventModels.PersonDetails OldDetails { get; init; }
    public required EventModels.TrnRequestMetadata TrnRequestMetadata { get; init; }
    public required string? DetailsChangeReasonDetail { get; init; }
    public required EventModels.File? DetailsChangeEvidenceFile { get; init; }
    public required PersonDetailsUpdatedFromTrnRequestEventChanges Changes { get; init; }
}

[Flags]
public enum PersonDetailsUpdatedFromTrnRequestEventChanges
{
    None = 0,
    DateOfBirth = 1 << 0,
    EmailAddress = 1 << 1,
    NationalInsuranceNumber = 1 << 2,
}
