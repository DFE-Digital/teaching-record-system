namespace TeachingRecordSystem.Core.Events.Models;

public record Alert
{
    public required Guid AlertId { get; init; }
    public required Guid? AlertTypeId { get; init; }  // Nullable as pre-migration audit records won't have an alert type
    public required string? Details { get; init; }
    public required string? ExternalLink { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }

    public bool? DqtSpent { get; init; }
    public AlertDqtSanctionCode? DqtSanctionCode { get; init; }

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

public record AlertDqtSanctionCode
{
    public required string Name { get; init; }
    public required string Value { get; init; }
}
