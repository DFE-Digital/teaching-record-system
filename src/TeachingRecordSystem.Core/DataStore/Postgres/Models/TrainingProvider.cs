namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TrainingProvider
{
    public const string UkprnIndexName = "ix_training_provider_ukprn";

    public required Guid TrainingProviderId { get; init; }
    public required string? Ukprn { get; set; }
    public required string Name { get; set; }
    public required bool IsActive { get; set; }
}
