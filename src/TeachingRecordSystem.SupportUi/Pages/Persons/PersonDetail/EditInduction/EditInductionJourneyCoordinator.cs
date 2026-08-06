using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[JourneyCoordinator(JourneyNames.EditInduction, routeValueKeys: ["personId"])]
public class EditInductionJourneyCoordinator(
    IDbContextFactory<TrsDbContext> dbContextFactory,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : JourneyCoordinator<EditInductionState>
{
    public Guid PersonId => HttpContext.GetCurrentPersonFeature().PersonId;

    public string PageCaption => $"Edit induction details - {HttpContext.GetCurrentPersonFeature().Name}";

    public InductionStatus Status => State.InductionStatus;

    // Where the journey returns to, and where cancelling and the first question's back link go.
    public string InductionUrl => linkGenerator.Persons.PersonDetail.Induction(PersonId);

    public override async Task<EditInductionState> GetStartingStateAsync()
    {
        // The journey filter runs while the request-scoped DbContext is in use by the filters around
        // it, so this takes its own context rather than sharing one mid-request.
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var person = await dbContext.Persons.SingleAsync(p => p.PersonId == PersonId);

        return new EditInductionState
        {
            InductionStatus = person.InductionStatus,
            CurrentInductionStatus = person.InductionStatus,
            StartDate = person.InductionStartDate,
            CompletedDate = person.InductionCompletedDate,
            ExemptionReasonIds = person.InductionExemptionReasonIds
        };
    }

    public string StatusUrl(string? returnUrl = null) =>
        linkGenerator.Persons.PersonDetail.EditInduction.Status(InstanceId, returnUrl);

    public string ExemptionReasonsUrl(string? returnUrl = null) =>
        linkGenerator.Persons.PersonDetail.EditInduction.ExemptionReasons(InstanceId, returnUrl);

    public string StartDateUrl(string? returnUrl = null) =>
        linkGenerator.Persons.PersonDetail.EditInduction.StartDate(InstanceId, returnUrl);

    public string CompletedDateUrl(string? returnUrl = null) =>
        linkGenerator.Persons.PersonDetail.EditInduction.CompletedDate(InstanceId, returnUrl);

    public string ReasonUrl(string? returnUrl = null) =>
        linkGenerator.Persons.PersonDetail.EditInduction.Reason(InstanceId, returnUrl);

    public string CheckAnswersUrl() =>
        linkGenerator.Persons.PersonDetail.EditInduction.CheckAnswers(InstanceId);

    // The four questions the induction page links to are each a way into the journey, so the one the
    // user came in by is step 0 of the path — it doesn't need recording in the state. Check answers
    // uses this to decide which of its answers offer a Change link: a journey started at 'change
    // start date' can't also change the status.
    public bool StartedAtStatus => StartedAt(StatusUrl());

    public bool StartedAtStartDate => StartedAt(StartDateUrl());

    // The question the user came in by, which is where an unfinished journey sends them back to.
    public string JourneyStartUrl =>
        Path.Steps.Count > 0 ? Path.Steps.First().GetUrl(InstanceId) : InductionUrl;

    // A question the status has stopped asking for — because the status changed, or because the
    // request named the page directly — sends the user back to the question they came in by. The
    // default is the last step, which for this journey is check answers, and landing there with the
    // question unanswered just bounces them out again.
    public override IResult OnInvalidStep() => Results.Redirect(JourneyStartUrl);

    // The next question after the status, which is what the status decides.
    public string NextQuestionAfterStatus() =>
        Status.RequiresExemptionReasons() ? ExemptionReasonsUrl()
            : Status.RequiresStartDate() ? StartDateUrl()
            : ReasonUrl();

    public string NextQuestionAfterStartDate() =>
        Status.RequiresCompletedDate() ? CompletedDateUrl() : ReasonUrl();

    // Records the status and moves on to the next question.
    //
    // The status decides which questions the journey asks, so answering it can add and remove them
    // rather than simply moving the user on. When that happens the steps after this one no longer
    // describe the journey, so they're dropped and the user works forward through the questions
    // again — the path only ever holds the questions they've been through. Otherwise the path is left
    // alone, so a change made from check answers takes the user straight back there.
    public string AnswerStatusAndAdvance(Action<EditInductionState> updateState)
    {
        var questionsBefore = QuestionUrls();
        UpdateState(updateState);
        var questionsAfter = QuestionUrls();

        var nextQuestionUrl = NextQuestionAfterStatus();

        if (!questionsAfter.SequenceEqual(questionsBefore))
        {
            return AdvanceTo(nextQuestionUrl, new PushStepOptions { SetAsLastStep = true }).Url;
        }

        // Unlike the other questions, the status doesn't send the user back to check answers when they
        // came from there to change it: the answers that follow depend on it, so they walk forward
        // through them again even when the status they picked is the one it already had.
        return AdvanceToQuestion(nextQuestionUrl);
    }

    // Pushes a question onto the path and returns its URL, ignoring any returnUrl on the current
    // request. AdvanceTo would honour that returnUrl and take the user straight back to check answers,
    // which is wrong when the answer they've just given means there's another question to ask first.
    public string AdvanceToQuestion(string questionUrl)
    {
        AdvanceTo(questionUrl);
        return questionUrl;
    }

    // Where the journey goes once a question has been answered.
    //
    // Going back to check answers must not push a step: PushStep truncates everything after the
    // current one, so advancing from an early question straight to check answers would drop the
    // questions its other change links point at. When the target is already in the path a plain
    // redirect leaves the path intact.
    public string ContinueTo(string nextQuestionUrl)
    {
        // The user came here from check answers to change a single answer.
        if (ReturnUrl is string returnUrl)
        {
            return returnUrl;
        }

        return AdvanceTo(nextQuestionUrl).Url;
    }

    // Where the user goes back to once they've answered the question they came in to change, when
    // that's somewhere the journey has already been.
    public string? ReturnUrl =>
        HttpContext.Request.Query[ReturnUrlQueryParameterName].ToString() is { Length: > 0 } returnUrl &&
        this.IsLocalUrl(returnUrl) &&
        PathContains(returnUrl)
            ? returnUrl
            : null;

    private bool PathContains(string url) => Path.ContainsStep(CreateStepFromUrl(url));

    // Every answer the change needs has been given, so check answers has something to show. The
    // journey has four entry points and check answers offers Change links, so unlike a journey that
    // can only be walked front to back this guard is reachable and does real work.
    public bool IsComplete =>
        State.InductionStatus != InductionStatus.None &&
        (!State.InductionStatus.RequiresStartDate() || State.StartDate.HasValue) &&
        (!State.InductionStatus.RequiresCompletedDate() || State.CompletedDate.HasValue) &&
        (!State.InductionStatus.RequiresExemptionReasons() || State.ExemptionReasonIds.Length != 0) &&
        State.ChangeReason.HasValue &&
        (State.ChangeReason == PersonInductionChangeReason.AnotherReason) == !string.IsNullOrEmpty(State.ChangeReasonDetail) &&
        State.ProvideAdditionalInformation is bool provideAdditionalInformation &&
        provideAdditionalInformation == !string.IsNullOrEmpty(State.AdditionalInformation) &&
        State.Evidence.IsComplete;

    // Returns the URL to send the user back to.
    public async Task<string> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(State.Evidence.UploadedEvidenceFile);
        DeleteInstance();
        return InductionUrl;
    }

    // The questions this status asks, in the order they're asked.
    private string[] QuestionUrls()
    {
        List<string> urls = [StatusUrl()];

        if (Status.RequiresExemptionReasons())
        {
            urls.Add(ExemptionReasonsUrl());
        }

        if (Status.RequiresStartDate())
        {
            urls.Add(StartDateUrl());
        }

        if (Status.RequiresCompletedDate())
        {
            urls.Add(CompletedDateUrl());
        }

        urls.AddRange([ReasonUrl(), CheckAnswersUrl()]);

        return urls.ToArray();
    }

    private bool StartedAt(string pageUrl) =>
        Path.Steps.Count > 0 && Path.Steps.First().StepId == CreateStepFromUrl(pageUrl).StepId;
}
