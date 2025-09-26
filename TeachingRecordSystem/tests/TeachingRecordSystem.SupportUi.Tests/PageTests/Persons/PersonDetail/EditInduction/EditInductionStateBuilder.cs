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
    private string? EvidenceFileSizeDescription { get; set; }
    private Guid? EvidenceFileId { get; set; }
    private string? EvidenceFileName { get; set; }
    private InductionJourneyPage? JourneyStartPage { get; set; }
    private bool Initialized { get; set; }

    public EditInductionStateBuilder WithInitializedState(InductionStatus currentInductionStatus, InductionJourneyPage startPage)
    {
        this.Initialized = true;
        JourneyStartPage = startPage;
        InductionStatus = currentInductionStatus;
        CurrentInductionStatus = currentInductionStatus;
        return this;
    }

    public EditInductionStateBuilder WithUpdatedState(InductionStatus inductionStatus)
    {
        if (InductionStatus == InductionStatus.None)
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

    public EditInductionStateBuilder WithStartDate(DateOnly? date)
    {
        StartDate = date;
        return this;
    }

    public EditInductionStateBuilder WithCompletedDate(DateOnly? date)
    {
        CompletedDate = date;
        return this;
    }

    public EditInductionStateBuilder WithReasonChoice(InductionChangeReasonOption option)
    {
        ChangeReason = option;
        return this;
    }

    public EditInductionStateBuilder WithReasonDetailsChoice(bool addDetails, string? detailText = null)
    {
        HasAdditionalReasonDetail = addDetails;
        AdditionalReasonDetail = detailText;
        return this;
    }

    public EditInductionStateBuilder WithFileUploadChoice(bool uploadFile, Guid? evidenceFileId = null)
    {
        FileUpload = uploadFile;
        if (uploadFile)
        {
            EvidenceFileId = evidenceFileId ?? Guid.NewGuid();
            EvidenceFileName = "evidence.jpeg";
            EvidenceFileSizeDescription = "5MB";
        }
        return this;
    }

    public EditInductionState Build()
    {
        return new EditInductionState
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
            EvidenceFileId = EvidenceFileId,
            EvidenceFileName = EvidenceFileName,
            EvidenceFileSizeDescription = EvidenceFileSizeDescription,
            Initialized = Initialized
        };
    }
}
