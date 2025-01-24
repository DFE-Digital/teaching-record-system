using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public class EditInductionState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditInduction,
        typeof(EditInductionState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public InductionJourneyPage? JourneyStartPage { get; set; }
    public InductionStatus InductionStatus { get; set; }
    public InductionStatus CurrentInductionStatus { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public Guid[]? ExemptionReasonIds { get; set; } = Array.Empty<Guid>();
    public InductionChangeReasonOption? ChangeReason { get; set; }
    public bool? HasAdditionalReasonDetail { get; set; }
    public string? ChangeReasonDetail { get; set; }
    public bool? UploadEvidence { get; set; }
    public Guid? EvidenceFileId { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceFileSizeDescription { get; set; }

    public bool Initialized { get; set; }
    [JsonIgnore]
    public bool IsComplete =>
        InductionStatus != InductionStatus.None &&
        (!InductionStatus.RequiresStartDate() || StartDate.HasValue) &&
        (!InductionStatus.RequiresCompletedDate() || CompletedDate.HasValue) &&
        (!InductionStatus.RequiresExemptionReasons() || (ExemptionReasonIds != null && ExemptionReasonIds.Any())) &&
        ChangeReason.HasValue &&
        HasAdditionalReasonDetail.HasValue &&
        UploadEvidence.HasValue &&
        (!UploadEvidence.Value || (UploadEvidence.Value && EvidenceFileId.HasValue));

    public async Task EnsureInitializedAsync(TrsDbContext dbContext, Guid personId, InductionJourneyPage startPage)
    {
        if (Initialized)
        {
            return;
        }
        var person = await dbContext.Persons
            .SingleAsync(q => q.PersonId == personId);

        InductionStatus = person.InductionStatus;
        //StartDate = person.InductionStartDate;
        //CompletedDate = person.InductionCompletedDate;
        if (JourneyStartPage == null)
        {
            JourneyStartPage = startPage;
        }

        Initialized = true;
    }
}
