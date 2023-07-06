namespace TeachingRecordSystem.Core.Events;

public record InternationalQtsAwardedEmailSentEvent : EventBase
{
    public required Guid InternationalQtsAwardedEmailsJobId { get; set; }
    public required Guid PersonId { get; set; }
    public required string EmailAddress { get; set; }
}
