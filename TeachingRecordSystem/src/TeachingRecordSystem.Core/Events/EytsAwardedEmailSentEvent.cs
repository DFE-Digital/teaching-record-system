namespace TeachingRecordSystem.Core.Events;

public record EytsAwardedEmailSentEvent : EventBase, IEventWithPersonId
{
    public required Guid EytsAwardedEmailsJobId { get; init; }
    public required Guid PersonId { get; init; }
    public required string EmailAddress { get; init; }
}
