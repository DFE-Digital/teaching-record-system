namespace TeachingRecordSystem.Core.Jobs;

public class BatchSendProfessionalStatusEmailsOptions
{
    public required DateTime InitialLastHoldsFromEndUtc { get; init; }
    public required int EmailDelayDays { get; init; }
    public required string JobSchedule { get; init; }
    public required Guid[] RaisedByUserIds { get; init; } = [];
}
