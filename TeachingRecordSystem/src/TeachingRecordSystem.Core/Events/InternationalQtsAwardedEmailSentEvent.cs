namespace TeachingRecordSystem.Core.Events;

public record InternationalQtsAwardedEmailSentEvent : EventBase, IEventWithPersonId
{
    public required Guid InternationalQtsAwardedEmailsJobId { get; init; }
    public required Guid PersonId { get; init; }
    public required string EmailAddress { get; init; }
}
