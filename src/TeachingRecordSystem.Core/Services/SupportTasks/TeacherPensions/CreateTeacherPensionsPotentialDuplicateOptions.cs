using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.SupportTasks.TeacherPensions;

public record CreateTeacherPensionsPotentialDuplicateOptions
{
    public required Guid PersonId { get; init; }
    public required TrnRequestMetadata TrnRequest { get; init; }
    public required string FileName { get; init; }
    public required long IntegrationTransactionId { get; init; }
}
