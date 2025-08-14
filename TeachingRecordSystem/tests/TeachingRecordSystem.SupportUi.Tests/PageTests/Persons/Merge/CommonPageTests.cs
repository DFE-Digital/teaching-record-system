using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Merge;

[Collection(nameof(DisableParallelization))]
public class CommonPageTests : MergeTestBase
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
        new[] { "enter-trn", "matches", "merge", "check-answers" },
        TestHttpMethods.GetAndPost)]
    public async Task FeatureFlagDisabled_ReturnsNotFound(string page, HttpMethod httpMethod)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);

        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .Build());

        // Act
        var request = new HttpRequestMessage(httpMethod, GetRequestPath(personA, page, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);

        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        new[] { "matches", "merge", "check-answers" },
        TestHttpMethods.GetAndPost)]
    public async Task OtherTrnNotSelected_RedirectsToEnterTrnPage(string page, HttpMethod httpMethod)
    {
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .Build());

        var request = new HttpRequestMessage(httpMethod, GetRequestPath(personA, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/merge/enter-trn?{journeyInstance.GetUniqueIdQueryParameter()}");
    }

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        new[] { "merge", "check-answers" },
        TestHttpMethods.GetAndPost)]
    public async Task PrimaryPersonNotSelected_RedirectsToMatches(string page, HttpMethod httpMethod)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
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

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        "check-answers",
        TestHttpMethods.GetAndPost)]
    public async Task PersonAttributeSourcesNotSet_RedirectsToMerge(string page, HttpMethod httpMethod)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
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

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        "check-answers",
        TestHttpMethods.GetAndPost)]
    public async Task UploadEvidenceNotSet_RedirectsToMerge(string page, HttpMethod httpMethod)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
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

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        "check-answers",
        TestHttpMethods.GetAndPost)]
    public async Task UploadEvidenceSetToTrue_ButEvidenceFileNotUploaded_RedirectsToMerge(string page, HttpMethod httpMethod)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
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

    [Theory]
    [InlineData("enter-trn", null)]
    [InlineData("matches", "enter-trn")]
    [InlineData("merge", "matches")]
    [InlineData("check-answers", "merge")]
    public async Task Get_BacklinkLinksToExpected(string page, string? expectedPage)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
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

    [Theory]
    [InlineData("enter-trn", "check-answers")]
    [InlineData("matches", "check-answers")]
    [InlineData("merge", "check-answers")]
    [InlineData("check-answers", "merge")]
    public async Task Get_FromCheckAnswers_BacklinkLinksToExpected(string page, string? expectedPage)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
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

    [Theory]
    [InlineData("enter-trn", "Continue", "Cancel and return to record")]
    [InlineData("matches", "Continue", "Cancel and return to record")]
    [InlineData("merge", "Continue", "Cancel and return to record")]
    [InlineData("check-answers", "Confirm and update primary record", "Cancel")]
    public async Task Get_ContinueAndCancelButtons_ExistOnPage(string page, string continueButtonText, string cancelButtonText)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
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

    [Theory]
    [InlineData("enter-trn")]
    [InlineData("matches")]
    [InlineData("merge")]
    [InlineData("check-answers")]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetailPage(string page)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
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

    [Theory]
    [InlineData("enter-trn")]
    [InlineData("matches")]
    [InlineData("merge")]
    [InlineData("check-answers")]
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
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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

    [Theory]
    [InlineData("enter-trn")]
    [InlineData("matches")]
    [InlineData("merge")]
    [InlineData("check-answers")]
    public async Task Post_PersonAHasOpenAlert_ReturnsBadRequest(string page)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(p => p
            .WithAlert(a => a.WithEndDate(null)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        new[] { "enter-trn", "matches", "merge", "check-answers" },
        new[] { InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed })]
    public async Task Post_PersonAHasInvalidInductionStatus_ReturnsBadRequest(string page, InductionStatus status)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(p => p
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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

    [Theory]
    [InlineData("matches")]
    [InlineData("merge")]
    [InlineData("check-answers")]
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
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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

    [Theory]
    [InlineData("matches")]
    [InlineData("merge")]
    [InlineData("check-answers")]
    public async Task Post_PersonBHasOpenAlert_ReturnsBadRequest(string page)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(configurePersonB: p => p
            .WithAlert(a => a.WithEndDate(null)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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

    [Theory]
    [MemberData(nameof(AllCombinationsOf),
        new[] { "matches", "merge", "check-answers" },
        new[] { InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed })]
    public async Task Post_PersonBHasInvalidInductionStatus_ReturnsBadRequest(string page, InductionStatus status)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(configurePersonB: p => p
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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

    [Theory]
    [InlineData("enter-trn", "matches")]
    [InlineData("matches", "merge")]
    [InlineData("merge", "check-answers")]
    [InlineData("check-answers", null)]
    public async Task Post_RedirectsToExpected(string page, string? expectedPage)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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

    [Theory]
    [InlineData("enter-trn", "check-answers")]
    [InlineData("matches", "check-answers")]
    [InlineData("merge", "check-answers")]
    [InlineData("check-answers", null)]
    public async Task Post_FromCheckAnswers_RedirectsToExpected(string page, string? expectedPage)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance, fromCheckAnswers: true))
        {
            Content = new MergePostRequestContentBuilder()
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

    private string GetRequestPath(TestData.CreatePersonResult person, string page, JourneyInstance<MergeState>? journeyInstance = null, bool? fromCheckAnswers = null) =>
        $"/persons/{person.PersonId}/merge/{page}?{journeyInstance?.GetUniqueIdQueryParameter()}{(fromCheckAnswers is bool f ? $"&fromCheckAnswers={f}" : "")}";

    private Task<JourneyInstance<MergeState>> CreateJourneyInstanceAsync(Guid personId, MergeState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.MergePerson,
            state ?? new MergeState(),
            new KeyValuePair<string, object>("personId", personId));
}
