namespace TeachingRecordSystem.Core.Models.SupportTasks;

public record TrnRequestManualChecksNeededData : ISupportTaskData
{
    string ISupportTaskData.GetOutcomeLabel() => "Completed";
}
