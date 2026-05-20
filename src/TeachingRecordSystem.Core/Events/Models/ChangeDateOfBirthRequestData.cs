using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Events.Models;

public record ChangeDateOfBirthRequestData
{
    public required DateOnly DateOfBirth { get; init; }
    public required Guid EvidenceFileId { get; init; }
    public required string EvidenceFileName { get; init; }
    public required string? EmailAddress { get; init; }
    public required SupportRequestOutcome? ChangeRequestOutcome { get; init; }

    public static ChangeDateOfBirthRequestData FromModel(Core.Models.SupportTasks.ChangeDateOfBirthRequestData model) =>
        new()
        {
            DateOfBirth = model.DateOfBirth,
            EvidenceFileId = model.EvidenceFileId,
            EvidenceFileName = model.EvidenceFileName,
            EmailAddress = model.EmailAddress,
            ChangeRequestOutcome = model.ChangeRequestOutcome
        };
}
