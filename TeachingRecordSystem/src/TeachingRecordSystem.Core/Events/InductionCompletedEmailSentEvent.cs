namespace TeachingRecordSystem.Core.Events;

public record InductionCompletedEmailSentEvent : EventBase
{
    public required Guid InductionCompletedEmailsJobId { get; set; }
    public required Guid PersonId { get; set; }
    public required string EmailAddress { get; set; }
}
