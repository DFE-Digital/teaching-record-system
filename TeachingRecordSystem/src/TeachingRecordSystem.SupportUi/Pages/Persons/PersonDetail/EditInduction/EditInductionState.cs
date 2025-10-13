using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

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
    public Guid[]? ExemptionReasonIds { get; set; } = [];
    public InductionChangeReasonOption? ChangeReason { get; set; }
    public bool? HasAdditionalReasonDetail { get; set; }
    public string? ChangeReasonDetail { get; set; }
    public EvidenceUploadModel Evidence { get; set; } = new();

    public bool Initialized { get; set; }

    [JsonIgnore]
    public bool IsComplete =>
        InductionStatus != InductionStatus.None &&
        (!InductionStatus.RequiresStartDate() || StartDate.HasValue) &&
        (!InductionStatus.RequiresCompletedDate() || CompletedDate.HasValue) &&
        (!InductionStatus.RequiresExemptionReasons() || (ExemptionReasonIds != null && ExemptionReasonIds.Length != 0)) &&
        ChangeReason.HasValue &&
        HasAdditionalReasonDetail.HasValue &&
        Evidence.IsComplete;

    public async Task EnsureInitializedAsync(TrsDbContext dbContext, Guid personId, InductionJourneyPage startPage)
    {
        if (Initialized)
        {
            return;
        }

        var person = await dbContext.Persons.SingleAsync(q => q.PersonId == personId);

        CurrentInductionStatus = person.InductionStatus;
        JourneyStartPage = startPage;
        StartDate = person.InductionStartDate;
        CompletedDate = person.InductionCompletedDate;
        ExemptionReasonIds = person.InductionExemptionReasonIds;
        if (InductionStatus == InductionStatus.None)
        {
            InductionStatus = CurrentInductionStatus;
        }

        Initialized = true;
    }
}
