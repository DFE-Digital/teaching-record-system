namespace TeachingRecordSystem.Core.Jobs.Scheduling;

public class EwcWalesImportJobOptions
{
    public required string JobSchedule { get; init; } = "0 8 * * *";
}
