using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.SetStatus;

[Collection(nameof(DisableParallelization))]
public class CommonPageTests : SetStatusTestBase
{
    public CommonPageTests(HostFixture hostFixture) : base(hostFixture)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
    }

    public override void Dispose()
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);
        base.Dispose();
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        new[] { "change-reason", "check-answers" },
        new[] { PersonStatus.Deactivated, PersonStatus.Active },
        TestHttpMethods.GetAndPost)]
    public async Task FeatureFlagDisabled_ReturnsNotFound(string page, PersonStatus targetStatus, HttpMethod httpMethod)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);

        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(false);

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        // Act
        var request = new HttpRequestMessage(httpMethod, GetRequestPath(person, targetStatus, page, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);

        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        new[] { "change-reason", "check-answers" },
        new[] { PersonStatus.Deactivated, PersonStatus.Active },
        new[] { UserRoles.Viewer, UserRoles.AlertsManagerTra, UserRoles.AlertsManagerTraDbs, null },
        TestHttpMethods.GetAndPost)]
    public async Task UserDoesNotHavePermission_ReturnsForbidden(string page, PersonStatus targetStatus, string? role, HttpMethod httpMethod)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));

        var person = await CreatePersonToBecomeStatus(targetStatus);

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(false);

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        // Act
        var request = new HttpRequestMessage(httpMethod, GetRequestPath(person, targetStatus, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        new[] { "change-reason", "check-answers" },
        new[] { PersonStatus.Deactivated, PersonStatus.Active },
        TestHttpMethods.GetAndPost)]
    public async Task PersonIsAlreadyTargetStatus_ReturnsBadRequest(string page, PersonStatus targetStatus, HttpMethod httpMethod)
    {
        // Arrange
        var person = await CreatePersonWithCurrentStatus(targetStatus);

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(false);

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        // Act
        var request = new HttpRequestMessage(httpMethod, GetRequestPath(person, targetStatus, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        new[] { "change-reason", "check-answers" },
        TestHttpMethods.GetAndPost
    )]
    public async Task TargetStatusActive_PersonWasDeactivatedAsPartOfAMerge_ReturnsBadRequest(string page, HttpMethod httpMethod)
    {
        // Arrange
        var primaryPerson = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());
        var secondaryPerson = await CreatePersonWithCurrentStatus(PersonStatus.Deactivated, p => p
            .WithTrn()
            .WithMergedWithPersonId(primaryPerson.PersonId));

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(false)
            .WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);

        var journeyInstance = await CreateJourneyInstanceAsync(
            secondaryPerson.PersonId,
            stateBuilder.Build());

        // Act
        var request = new HttpRequestMessage(httpMethod, GetRequestPath(secondaryPerson, PersonStatus.Active, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        "check-answers",
        new[] { PersonStatus.Deactivated, PersonStatus.Active },
        TestHttpMethods.GetAndPost)]
    public async Task ReasonNotSet_RedirectsToChangeReason(string page, PersonStatus targetStatus, HttpMethod httpMethod)
    {
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(false)
            .Build());

        // Act
        var request = new HttpRequestMessage(httpMethod, GetRequestPath(person, targetStatus, page, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{person.PersonId}/set-status/{targetStatus}/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        "check-answers",
        new[] { PersonStatus.Deactivated, PersonStatus.Active },
        TestHttpMethods.GetAndPost)]
    public async Task ReasonSetToAnotherReason_ButReasonDetailNotProvided_RedirectsToChangeReason(string page, PersonStatus targetStatus, HttpMethod httpMethod)
    {
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(false);

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.AnotherReason, detailText: null);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.AnotherReason, detailText: null);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        // Act
        var request = new HttpRequestMessage(httpMethod, GetRequestPath(person, targetStatus, page, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{person.PersonId}/set-status/{targetStatus}/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        "check-answers",
        new[] { PersonStatus.Deactivated, PersonStatus.Active },
        TestHttpMethods.GetAndPost)]
    public async Task UploadEvidenceNotSet_RedirectsToChangeReason(string page, PersonStatus targetStatus, HttpMethod httpMethod)
    {
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState();

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        // Act
        var request = new HttpRequestMessage(httpMethod, GetRequestPath(person, targetStatus, page, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{person.PersonId}/set-status/{targetStatus}/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        "check-answers",
        new[] { PersonStatus.Deactivated, PersonStatus.Active },
        TestHttpMethods.GetAndPost)]
    public async Task UploadEvidenceSetToTrue_ButEvidenceFileNotUploaded_RedirectsToChangeReason(string page, PersonStatus targetStatus, HttpMethod httpMethod)
    {
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(true, evidenceFileId: null);

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        // Act
        var request = new HttpRequestMessage(httpMethod, GetRequestPath(person, targetStatus, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{person.PersonId}/set-status/{targetStatus}/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        new[] { "change-reason", "check-answers" },
        new[] { PersonStatus.Deactivated, PersonStatus.Active })]
    public async Task Get_PageTitle_CaptionIsExpected(string page, PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus, p => p
            .WithFirstName("Lily")
            .WithMiddleName("The")
            .WithLastName("Pink"));

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(false);

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, targetStatus, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var caption = doc.GetElementByTestId("set-status-caption");

        if (targetStatus == PersonStatus.Deactivated)
        {
            Assert.Equal("Deactivate record - Lily The Pink", caption!.TrimmedText());
        }
        else
        {
            Assert.Equal("Reactivate record - Lily The Pink", caption!.TrimmedText());
        }
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        "change-reason", null, new[] { PersonStatus.Active, PersonStatus.Deactivated })]
    [MemberData(nameof(AllCombinationsOf),
        "check-answers", "change-reason", new[] { PersonStatus.Active, PersonStatus.Deactivated })]
    public async Task Get_BacklinkLinksToExpected(string page, string? expectedPage, PersonStatus targetStatus)
    {
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(false);

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, targetStatus, page, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        var expectedBackLink = $"/persons/{person.PersonId}";
        if (expectedPage is not null)
        {
            expectedBackLink += $"/set-status/{targetStatus}/{expectedPage}?{journeyInstance?.GetUniqueIdQueryParameter()}";
        }
        Assert.Contains(expectedBackLink, backlink.Href);
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        "change-reason", "check-answers", new[] { PersonStatus.Active, PersonStatus.Deactivated })]
    [MemberData(nameof(AllCombinationsOf),
        "check-answers", "change-reason", new[] { PersonStatus.Active, PersonStatus.Deactivated })]
    public async Task Get_FromCheckAnswers_BacklinkLinksToExpected(string page, string? expectedPage, PersonStatus targetStatus)
    {
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(false);

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, targetStatus, page, journeyInstance, fromCheckAnswers: true));
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        var expectedBackLink = $"/persons/{person.PersonId}";
        if (expectedPage is not null)
        {
            expectedBackLink += $"/set-status/{targetStatus}/{expectedPage}?{journeyInstance?.GetUniqueIdQueryParameter()}";
        }
        Assert.Contains(expectedBackLink, backlink.Href);
    }

    [Theory]
    [InlineData("change-reason", PersonStatus.Active, "Continue", "Cancel and return to record")]
    [InlineData("change-reason", PersonStatus.Deactivated, "Continue", "Cancel and return to record")]
    [InlineData("check-answers", PersonStatus.Active, "Confirm and reactivate record", "Cancel")]
    [InlineData("check-answers", PersonStatus.Deactivated, "Confirm and deactivate record", "Cancel")]
    public async Task Get_ContinueAndCancelButtons_ExistOnPage(string page, PersonStatus targetStatus, string continueButtonText, string cancelButtonText)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(false);

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, targetStatus, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        var buttons = form.GetElementsByTagName("button").OfType<IHtmlButtonElement>();
        Assert.Collection(buttons,
            b => Assert.Equal(continueButtonText, b.TrimmedText()),
            b => Assert.Equal(cancelButtonText, b.TrimmedText()));
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        new[] { "change-reason", "check-answers" },
        new[] { PersonStatus.Deactivated, PersonStatus.Active })]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetailPage(string page, PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(false);

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, targetStatus, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, cancelButton!.FormAction);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        AssertEx.ResponseIsRedirectTo(redirectResponse, $"/persons/{person.PersonId}");
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        "change-reason", "check-answers", new[] { PersonStatus.Deactivated, PersonStatus.Active })]
    [MemberData(nameof(AllCombinationsOf),
        "check-answers", null, new[] { PersonStatus.Deactivated, PersonStatus.Active })]
    public async Task Post_RedirectsToExpected(string page, string? expectedPage, PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(false)
            .WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord)
            .WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, page, journeyInstance))
        {
            Content = new SetStatusPostRequestContentBuilder()
                .WithDeactivateReason(DeactivateReasonOption.ProblemWithTheRecord)
                .WithReactivateReason(ReactivateReasonOption.DeactivatedByMistake)
                .WithEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var expectedRedirect = $"/persons/{person.PersonId}";
        if (expectedPage is not null)
        {
            expectedRedirect += $"/set-status/{targetStatus}/{expectedPage}?{journeyInstance?.GetUniqueIdQueryParameter()}";
        }

        AssertEx.ResponseIsRedirectTo(response, expectedRedirect);
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        "change-reason", "check-answers", new[] { PersonStatus.Deactivated, PersonStatus.Active })]
    [MemberData(nameof(AllCombinationsOf),
        "check-answers", null, new[] { PersonStatus.Deactivated, PersonStatus.Active })]
    public async Task Post_FromCheckAnswers_RedirectsToExpected(string page, string? expectedPage, PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(false)
            .WithDeactivateReasonChoice(DeactivateReasonOption.ProblemWithTheRecord)
            .WithReactivateReasonChoice(ReactivateReasonOption.DeactivatedByMistake);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, page, journeyInstance, fromCheckAnswers: true))
        {
            Content = new SetStatusPostRequestContentBuilder()
                .WithDeactivateReason(DeactivateReasonOption.ProblemWithTheRecord)
                .WithReactivateReason(ReactivateReasonOption.DeactivatedByMistake)
                .WithEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var expectedRedirect = $"/persons/{person.PersonId}";
        if (expectedPage is not null)
        {
            expectedRedirect += $"/set-status/{targetStatus}/{expectedPage}?{journeyInstance?.GetUniqueIdQueryParameter()}";
        }

        AssertEx.ResponseIsRedirectTo(response, expectedRedirect);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, PersonStatus targetStatus, string page, JourneyInstance<SetStatusState>? journeyInstance = null, bool? fromCheckAnswers = null) =>
        $"/persons/{person.PersonId}/set-status/{targetStatus}/{page}?{journeyInstance?.GetUniqueIdQueryParameter()}{(fromCheckAnswers is bool f ? $"&fromCheckAnswers={f}" : "")}";

    private Task<JourneyInstance<SetStatusState>> CreateJourneyInstanceAsync(Guid personId, SetStatusState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.SetStatus,
            state ?? new SetStatusState(),
            new KeyValuePair<string, object>("personId", personId));
}
