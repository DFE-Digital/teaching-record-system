using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Events.Models;

public record ChangeNameRequestData
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required Guid EvidenceFileId { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string? EmailAddress { get; init; }
    public required SupportRequestOutcome? ChangeRequestOutcome { get; init; }

    public static ChangeNameRequestData FromModel(Core.Models.SupportTasks.ChangeNameRequestData model) =>
        new()
        {
            FirstName = model.FirstName,
            MiddleName = model.MiddleName,
            LastName = model.LastName,
            EvidenceFileId = model.EvidenceFileId,
            EvidenceFileName = model.EvidenceFileName,
            EmailAddress = model.EmailAddress,
            ChangeRequestOutcome = model.ChangeRequestOutcome
        };
}
