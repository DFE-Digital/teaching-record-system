namespace TeachingRecordSystem.Core.Events;

public record InductionCompletedEmailSentEvent : EventBase, IEventWithPersonId
{
    public required Guid InductionCompletedEmailsJobId { get; init; }
    public required Guid PersonId { get; init; }
    public required string EmailAddress { get; init; }
}
