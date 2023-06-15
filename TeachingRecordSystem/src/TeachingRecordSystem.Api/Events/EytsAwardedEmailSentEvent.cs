namespace TeachingRecordSystem.Api.Events;

public record EytsAwardedEmailSentEvent : EventBase
{
    public required Guid EytsAwardedEmailsJobId { get; set; }
    public required Guid PersonId { get; set; }
    public required string EmailAddress { get; set; }
}
