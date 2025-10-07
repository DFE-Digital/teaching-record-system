using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.MergePerson;

public class CommonPageTests(HostFixture hostFixture) : MergePersonTestBase(hostFixture)
{
    [Test]
    [MatrixDataSource]
    public async Task OtherTrnNotSelected_RedirectsToEnterTrnPage(
        [Matrix("matches", "merge", "check-answers")] string page,
        [MatrixHttpMethods(TestHttpMethods.GetAndPost)] HttpMethod httpMethod)
    {
        var personA = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .Build());

        var request = new HttpRequestMessage(httpMethod, GetRequestPath(personA, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/merge/enter-trn?{journeyInstance.GetUniqueIdQueryParameter()}");
    }

    [Test]
    [MatrixDataSource]
    public async Task PrimaryPersonNotSelected_RedirectsToMatches(
        [Matrix("merge", "check-answers")] string page,
        [MatrixHttpMethods(TestHttpMethods.GetAndPost)] HttpMethod httpMethod)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var request = new HttpRequestMessage(httpMethod, GetRequestPath(personA, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/merge/matches?{journeyInstance.GetUniqueIdQueryParameter()}");
    }

    [Test]
    [MatrixDataSource]
    public async Task PersonAttributeSourcesNotSet_RedirectsToMerge(
        [Matrix("check-answers")] string page,
        [MatrixHttpMethods(TestHttpMethods.GetAndPost)] HttpMethod httpMethod)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(httpMethod, GetRequestPath(personA, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/merge/merge?{journeyInstance.GetUniqueIdQueryParameter()}");
    }

    [Test]
    [MatrixDataSource]
    public async Task UploadEvidenceNotSet_RedirectsToMerge(
        [Matrix("check-answers")] string page,
        [MatrixHttpMethods(TestHttpMethods.GetAndPost)] HttpMethod httpMethod)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .Build());

        var request = new HttpRequestMessage(httpMethod, GetRequestPath(personA, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/merge/merge?{journeyInstance.GetUniqueIdQueryParameter()}");
    }

    [Test]
    [MatrixDataSource]
    public async Task UploadEvidenceSetToTrue_ButEvidenceFileNotUploaded_RedirectsToMerge(
        [Matrix("check-answers")] string page,
        [MatrixHttpMethods(TestHttpMethods.GetAndPost)] HttpMethod httpMethod)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(true, evidenceFileId: null)
                .Build());

        var request = new HttpRequestMessage(httpMethod, GetRequestPath(personA, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/merge/merge?{journeyInstance.GetUniqueIdQueryParameter()}");
    }

    [Test]
    [Arguments("enter-trn", null)]
    [Arguments("matches", "enter-trn")]
    [Arguments("merge", "matches")]
    [Arguments("check-answers", "merge")]
    public async Task Get_BacklinkLinksToExpected(string page, string? expectedPage)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, page, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        var expectedBackLink = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedBackLink += "/merge/" + expectedPage;
        }
        Assert.Contains(expectedBackLink, backlink.Href);
    }

    [Test]
    [Arguments("enter-trn", "check-answers")]
    [Arguments("matches", "check-answers")]
    [Arguments("merge", "check-answers")]
    [Arguments("check-answers", "merge")]
    public async Task Get_FromCheckAnswers_BacklinkLinksToExpected(string page, string? expectedPage)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, page, journeyInstance, fromCheckAnswers: true));
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        var expectedBackLink = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedBackLink += "/merge/" + expectedPage;
        }
        Assert.Contains(expectedBackLink, backlink.Href);
    }

    [Test]
    [Arguments("enter-trn", "Continue", "Cancel and return to record")]
    [Arguments("matches", "Continue", "Cancel and return to record")]
    [Arguments("merge", "Continue", "Cancel and return to record")]
    [Arguments("check-answers", "Confirm and update primary record", "Cancel")]
    public async Task Get_ContinueAndCancelButtons_ExistOnPage(string page, string continueButtonText, string cancelButtonText)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, page, journeyInstance));
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

    [Test]
    [Arguments("enter-trn")]
    [Arguments("matches")]
    [Arguments("merge")]
    [Arguments("check-answers")]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetailPage(string page)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, page, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, cancelButton!.FormAction);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        AssertEx.ResponseIsRedirectTo(redirectResponse, $"/persons/{personA.PersonId}");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Test]
    [Arguments("enter-trn")]
    [Arguments("matches")]
    [Arguments("merge")]
    [Arguments("check-answers")]
    public async Task Post_PersonAIsDeactivated_ReturnsBadRequest(string page)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(personA.Person);
            personA.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    [Arguments("enter-trn")]
    [Arguments("matches")]
    [Arguments("merge")]
    [Arguments("check-answers")]
    public async Task Post_PersonAHasOpenAlert_ReturnsBadRequest(string page)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(p => p
            .WithAlert(a => a.WithEndDate(null)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    [MatrixDataSource]
    public async Task Post_PersonAHasInvalidInductionStatus_ReturnsBadRequest(
        [Matrix("enter-trn", "matches", "merge", "check-answers")] string page,
        [Matrix(InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed)] InductionStatus status)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(p => p
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    [Arguments("matches")]
    [Arguments("merge")]
    [Arguments("check-answers")]
    public async Task Post_PersonBIsDeactivated_ReturnsBadRequest(string page)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(personB.Person);
            personB.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    [Arguments("matches")]
    [Arguments("merge")]
    [Arguments("check-answers")]
    public async Task Post_PersonBHasOpenAlert_ReturnsBadRequest(string page)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(configurePersonB: p => p
            .WithAlert(a => a.WithEndDate(null)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    [MatrixDataSource]
    public async Task Post_PersonBHasInvalidInductionStatus_ReturnsBadRequest(
        [Matrix("matches", "merge", "check-answers")] string page,
        [Matrix(InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed)] InductionStatus status)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(configurePersonB: p => p
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Test]
    [Arguments("enter-trn", "matches")]
    [Arguments("matches", "merge")]
    [Arguments("merge", "check-answers")]
    [Arguments("check-answers", null)]
    public async Task Post_RedirectsToExpected(string page, string? expectedPage)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var expectedRedirect = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedRedirect = $"{expectedRedirect}/merge/{expectedPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
        }

        AssertEx.ResponseIsRedirectTo(response, expectedRedirect);
    }

    [Test]
    [Arguments("enter-trn", "check-answers")]
    [Arguments("matches", "check-answers")]
    [Arguments("merge", "check-answers")]
    [Arguments("check-answers", null)]
    public async Task Post_FromCheckAnswers_RedirectsToExpected(string page, string? expectedPage)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergePersonStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance, fromCheckAnswers: true))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var expectedRedirect = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedRedirect = $"{expectedRedirect}/merge/{expectedPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
        }

        AssertEx.ResponseIsRedirectTo(response, expectedRedirect);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, string page, JourneyInstance<MergePersonState>? journeyInstance = null, bool? fromCheckAnswers = null) =>
        $"/persons/{person.PersonId}/merge/{page}?{journeyInstance?.GetUniqueIdQueryParameter()}{(fromCheckAnswers is bool f ? $"&fromCheckAnswers={f}" : "")}";

    private Task<JourneyInstance<MergePersonState>> CreateJourneyInstanceAsync(Guid personId, MergePersonState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.MergePerson,
            state ?? new MergePersonState(),
            new KeyValuePair<string, object>("personId", personId));
}
