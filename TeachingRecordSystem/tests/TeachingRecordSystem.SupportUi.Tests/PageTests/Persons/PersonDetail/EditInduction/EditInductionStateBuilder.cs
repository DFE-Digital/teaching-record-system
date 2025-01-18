using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditInductionStateBuilder
{
    private InductionStatus InductionStatus { get; set; }
    private InductionStatus CurrentInductionStatus { get; set; }
    private DateOnly? StartDate { get; set; }
    private DateOnly? CompletedDate { get; set; }
    private Guid[]? ExemptionReasonIds { get; set; }
    private InductionChangeReasonOption? ChangeReason { get; set; }
    private bool? HasAdditionalReasonDetail { get; set; }
    private string? AdditionalReasonDetail { get; set; }
    private bool? FileUpload { get; set; }
    private InductionJourneyPage? JourneyStartPage { get; set; }
    private bool Initialized { get; set; }

    public EditInductionStateBuilder WithInitialisedState(InductionStatus? currentInductionStatus, InductionJourneyPage startPage)
    {
        this.Initialized = true;
        JourneyStartPage = startPage;
        CurrentInductionStatus = currentInductionStatus ?? InductionStatus.None;
        InductionStatus = CurrentInductionStatus;
        return this;
    }

    public EditInductionStateBuilder WithUpdatedState(InductionStatus inductionStatus)
    {
        if (CurrentInductionStatus == InductionStatus.None)
        {
            throw new NotSupportedException("Initialised state must be set using WithInitialisedState");
        }
        InductionStatus = inductionStatus;
        return this;
    }

    public EditInductionStateBuilder WithExemptionReasonIds(Guid[] exemptionReasonIds)
    {
        ExemptionReasonIds = exemptionReasonIds;
        return this;
    }

    public EditInductionStateBuilder WithStartDate(DateOnly date)
    {
        StartDate = date;
        return this;
    }

    public EditInductionStateBuilder WithCompletedDate(DateOnly date)
    {
        CompletedDate = date;
        return this;
    }
    public EditInductionStateBuilder WithReasonChoice(InductionChangeReasonOption option)
    {
        ChangeReason = option;
        return this;
    }
    public EditInductionStateBuilder WithReasonDetailsChoice(bool addDetails, string detailText)
    {
        HasAdditionalReasonDetail = addDetails;
        AdditionalReasonDetail = detailText;
        return this;
    }
    public EditInductionStateBuilder WithFileUploadChoice(bool uploadFile)
    {
        FileUpload = uploadFile;
        return this;
    }

    public EditInductionState Create()
    {
        return new EditInductionState()
        {
            InductionStatus = InductionStatus,
            CurrentInductionStatus = CurrentInductionStatus,
            StartDate = StartDate,
            CompletedDate = CompletedDate,
            ExemptionReasonIds = ExemptionReasonIds,
            ChangeReason = ChangeReason,
            HasAdditionalReasonDetail = HasAdditionalReasonDetail,
            ChangeReasonDetail = AdditionalReasonDetail,
            UploadEvidence = FileUpload,
            JourneyStartPage = JourneyStartPage,
            Initialized = Initialized
        };
    }
}
