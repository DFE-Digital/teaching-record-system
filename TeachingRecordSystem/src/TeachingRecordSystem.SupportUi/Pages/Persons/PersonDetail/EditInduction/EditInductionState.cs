using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public class EditInductionState : IRegisterJourney
{
    public enum InductionJourneyPage
    {
        Status,
        ExemptionReason,
        StartDate,
        CompletedDate,
        ChangeReason
    }

    public static JourneyDescriptor Journey => new(
        JourneyNames.EditInduction,
        typeof(EditInductionState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public InductionStatus InductionStatus { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public InductionExemptionReasons? ExemptionReasons { get; set; }
    public string? ChangeReason { get; set; }
    public InductionJourneyPage? PageBreadcrumb { get; set; }

    public bool Initialized { get; set; }

    public async Task EnsureInitializedAsync(TrsDbContext dbContext, Guid personId)
    {
        if (Initialized)
        {
            return;
        }
        var person = await dbContext.Persons
            .SingleAsync(q => q.PersonId == personId);
        InductionStatus = person!.InductionStatus;

        Initialized = true;
    }
}
