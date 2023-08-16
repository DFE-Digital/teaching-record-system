namespace TeachingRecordSystem.Core.Events;

public record QtsAwardedEmailSentEvent : EventBase
{
    public required Guid QtsAwardedEmailsJobId { get; set; }
    public required Guid PersonId { get; set; }
    public required string EmailAddress { get; set; }
}
