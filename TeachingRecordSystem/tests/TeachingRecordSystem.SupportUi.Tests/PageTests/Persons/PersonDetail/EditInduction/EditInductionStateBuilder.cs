using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditInductionStateBuilder
{
    private InductionStatus InductionStatus { get; set; }
    private InductionStatus InitialInductionStatus { get; set; }
    private DateOnly? StartDate { get; set; }
    private DateOnly? CompletedDate { get; set; }
    private InductionExemptionReasons? ExemptionReasons { get; set; }
    private string? ChangeReason { get; set; }
    private InductionJourneyPage? JourneyStartPage { get; set; }
    private bool RecordManagedInCpd { get; set; }
    private bool Initialized { get; set; }

    public EditInductionStateBuilder WithInitialisedState(InductionStatus? initialInductionStatus, InductionJourneyPage startPage)
    {
        this.Initialized = true;
        JourneyStartPage = startPage;
        InitialInductionStatus = initialInductionStatus ?? InductionStatus.None;
        return this;
    }

    public EditInductionStateBuilder WithUpdatedState(InductionStatus inductionStatus)
    {
        if (InitialInductionStatus == InductionStatus.None)
        {
            throw new NotSupportedException("Initialised state must be set using WithInitialisedState");
        }
        InductionStatus = inductionStatus;
        return this;
    }

    public EditInductionState Create()
    {
        return new EditInductionState()
        {
            InductionStatus = InductionStatus,
            InitialInductionStatus = InitialInductionStatus,
            StartDate = StartDate,
            CompletedDate = CompletedDate,
            ExemptionReasons = ExemptionReasons,
            ChangeReason = ChangeReason,
            JourneyStartPage = JourneyStartPage,
            RecordManagedInCpd = RecordManagedInCpd,
            Initialized = Initialized
        };
    }
}
