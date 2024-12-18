using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

public class EditInductionState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditInduction,
        typeof(EditInductionState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public InductionStatus InductionStatus { get; set; }
    public InductionStatus InitialInductionStatus{ get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public InductionExemptionReasons? ExemptionReasons { get; set; }
    public string? ChangeReason { get; set; }
    public InductionJourneyPage? JourneyStartPage { get; set; }
    public bool RecordManagedInCpd { get; set; }

    public bool Initialized { get; set; }

    public async Task EnsureInitializedAsync(TrsDbContext dbContext, Guid personId, InductionJourneyPage startPage)
    {
        if (Initialized)
        {
            return;
        }
        var person = await dbContext.Persons
            .SingleAsync(q => q.PersonId == personId);

        InitialInductionStatus = person!.InductionStatus;
        if (JourneyStartPage == null)
        {
            JourneyStartPage = startPage;
        }

        Initialized = true;
    }
}
