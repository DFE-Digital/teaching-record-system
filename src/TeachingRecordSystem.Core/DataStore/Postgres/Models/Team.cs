namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Team
{
    public const int NameMaxLength = 200;

    public required Guid TeamId { get; init; }
    public required string Name { get; set; }
    public required DateTime CreatedOn { get; init; }
    public DateTime? UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
}
