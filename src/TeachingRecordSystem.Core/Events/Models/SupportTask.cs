using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Events.Models;

public record SupportTask
{
    public required string SupportTaskReference { get; init; }
    public required SupportTaskType SupportTaskType { get; init; }
    public required SupportTaskStatus Status { get; init; }
    public required string? OneLoginUserSubject { get; init; }
    public required Guid? PersonId { get; init; }
    public required ISupportTaskData Data { get; init; }
    public required SavedJourneyState? ResolveJourneySavedState { get; init; }
    public required Guid? AssignedToUserId { get; init; }
    public required string? OutcomeLabel { get; init; }
    public required string[] ZendeskTickets { get; init; }

    public static SupportTask FromModel(DataStore.Postgres.Models.SupportTask model) => new()
    {
        SupportTaskReference = model.SupportTaskReference,
        SupportTaskType = model.SupportTaskType,
        Status = model.Status,
        OneLoginUserSubject = model.OneLoginUserSubject,
        PersonId = model.PersonId,
        Data = model.Data,
        ResolveJourneySavedState = model.ResolveJourneySavedState,
        AssignedToUserId = model.AssignedToUserId,
        OutcomeLabel = model.OutcomeLabel,
        ZendeskTickets = model.ZendeskTickets
    };
}
