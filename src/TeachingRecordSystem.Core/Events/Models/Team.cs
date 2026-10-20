namespace TeachingRecordSystem.Core.Events.Models;

public record Team
{
    public required Guid TeamId { get; init; }
    public required string Name { get; init; }

    public static Team FromModel(DataStore.Postgres.Models.Team model) => new()
    {
        TeamId = model.TeamId,
        Name = model.Name
    };
}
