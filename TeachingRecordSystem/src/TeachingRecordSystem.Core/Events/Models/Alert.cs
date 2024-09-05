namespace TeachingRecordSystem.Core.Events.Models;

public record Alert
{
    public required Guid AlertId { get; init; }
    public required Guid AlertTypeId { get; init; }
    public required string? Details { get; init; }
    public required string? ExternalLink { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }

    public static Alert FromModel(DataStore.Postgres.Models.Alert model) => new()
    {
        AlertId = model.AlertId,
        AlertTypeId = model.AlertTypeId,
        Details = model.Details,
        ExternalLink = model.ExternalLink,
        StartDate = model.StartDate,
        EndDate = model.EndDate
    };
}
