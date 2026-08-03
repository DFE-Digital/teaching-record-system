using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[JourneyCoordinator(JourneyNames.EditDetails, routeValueKeys: ["personId"])]
public class EditDetailsJourneyCoordinator(
    PersonService personService,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : JourneyCoordinator<EditDetailsState>
{
    public override async Task<EditDetailsState> GetStartingStateAsync()
    {
        var personId = HttpContext.GetCurrentPersonFeature().PersonId;

        // The person feature doesn't carry Gender, so we need the record itself.
        var person = await personService.GetPersonAsync(personId) ??
            throw new InvalidOperationException($"Person '{personId}' was not found.");

        var emailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(person.EmailAddress);
        var nationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(person.NationalInsuranceNumber);

        return new EditDetailsState
        {
            OriginalFirstName = person.FirstName,
            OriginalMiddleName = person.MiddleName,
            OriginalLastName = person.LastName,
            OriginalDateOfBirth = person.DateOfBirth,
            OriginalEmailAddress = emailAddress,
            OriginalNationalInsuranceNumber = nationalInsuranceNumber,
            OriginalGender = person.Gender,
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            DateOfBirth = person.DateOfBirth,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = nationalInsuranceNumber,
            Gender = person.Gender
        };
    }

    /// <summary>
    /// Gets the URL of the reason question that still needs answering, or <see langword="null"/> if
    /// they've all been answered.
    /// </summary>
    public string? GetUnansweredReasonPageUrl(string? returnUrl = null)
    {
        if (!State.IsNameChangeReasonComplete)
        {
            return linkGenerator.Persons.PersonDetail.EditDetails.NameChangeReason(InstanceId, returnUrl);
        }

        if (!State.IsOtherDetailsChangeReasonComplete)
        {
            return linkGenerator.Persons.PersonDetail.EditDetails.OtherDetailsChangeReason(InstanceId, returnUrl);
        }

        return null;
    }

    /// <summary>
    /// Applies the page's answers and moves the journey on, to <paramref name="forwardUrl"/> when
    /// working through the journey normally.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Which question comes next depends on what the user changed, so a change made from the check
    /// answers page can leave a reason question unanswered. When that happens we send the user to
    /// that question rather than back to check answers, carrying the return URL so they still end up
    /// there once it's answered.
    /// </para>
    /// </remarks>
    public IActionResult AdvanceToNextQuestion(string forwardUrl, Action<EditDetailsState> updateState)
    {
        UpdateState(updateState);

        var returnUrl = ReturnUrl;
        var outstandingQuestionUrl = GetUnansweredReasonPageUrl(returnUrl);

        if (returnUrl is null)
        {
            return AdvanceTo(outstandingQuestionUrl ?? forwardUrl);
        }

        if (outstandingQuestionUrl is not null)
        {
            // Push the outstanding question onto the path and go there rather than letting AdvanceTo
            // return us to check answers.
            AdvanceTo(outstandingQuestionUrl);
            return new RedirectResult(outstandingQuestionUrl);
        }

        // Go back without rebuilding the path when check answers is still on it, so that the pages
        // its other change links point at stay reachable. Answering a question part-way through the
        // journey prunes the steps after it, in which case we advance so it's pushed back on.
        return StepIsValid(CreateStepFromUrl(returnUrl)) ? new RedirectResult(returnUrl) : AdvanceTo(returnUrl);
    }

    private string? ReturnUrl =>
        HttpContext.Request.Query.TryGetValue(ReturnUrlQueryParameterName, out var returnUrl) ? returnUrl.ToString() : null;

    /// <summary>
    /// Discards the journey along with any evidence files uploaded during it and returns the URL to
    /// send the user back to.
    /// </summary>
    public async Task<string> CancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(State.NameChangeEvidence.UploadedEvidenceFile);
        await evidenceUploadManager.DeleteUploadedFileAsync(State.OtherDetailsChangeEvidence.UploadedEvidenceFile);
        DeleteInstance();
        return linkGenerator.Persons.PersonDetail.Index(HttpContext.GetCurrentPersonFeature().PersonId);
    }
}
