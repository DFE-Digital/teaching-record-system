namespace TeachingRecordSystem.Core.Events;

public record QtsAwardedEmailSentEvent : EventBase, IEventWithPersonId
{
    public required Guid QtsAwardedEmailsJobId { get; init; }
    public required Guid PersonId { get; init; }
    public required string EmailAddress { get; init; }
}
