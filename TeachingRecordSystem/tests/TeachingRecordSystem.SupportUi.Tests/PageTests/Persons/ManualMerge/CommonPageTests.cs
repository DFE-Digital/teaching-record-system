using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.ManualMerge;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.ManualMerge;

[Collection(nameof(DisableParallelization))]
public class CommonPageTests : ManualMergeTestBase
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
    [InlineData("enter-trn")]
    [InlineData("matches")]
    [InlineData("merge")]
    [InlineData("check-answers")]
    public async Task Get_FeatureFlagDisabled_ReturnsNotFound(string page)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);

        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryRecord(personA)
                .WithAttributeSourcesSet()
                .Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, page, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);

        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
    }

    [Theory]
    [InlineData("matches")]
    [InlineData("merge")]
    [InlineData("check-answers")]
    public async Task Get_OtherTrnNotSelected_RedirectsToEnterTrnPage(string page)
    {
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/manual-merge/enter-trn?{journeyInstance.GetUniqueIdQueryParameter()}");
    }

    [Theory]
    [InlineData("merge")]
    [InlineData("check-answers")]
    public async Task Get_PrimaryRecordNotSelected_RedirectsToMatches(string page)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/manual-merge/matches?{journeyInstance.GetUniqueIdQueryParameter()}");
    }

    [Theory]
    [InlineData("check-answers")]
    public async Task Get_PersonAttributeSourcesNotSet_RedirectsToMerge(string page)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryRecord(personA)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/manual-merge/merge?{journeyInstance.GetUniqueIdQueryParameter()}");
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
                .WithPrimaryRecord(personA)
                .WithAttributeSourcesSet()
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
            expectedBackLink += "/manual-merge/" + expectedPage;
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
                .WithPrimaryRecord(personA)
                .WithAttributeSourcesSet()
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
            expectedBackLink += "/manual-merge/" + expectedPage;
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
                .WithPrimaryRecord(personA)
                .WithAttributeSourcesSet()
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
    public async Task Post_Cancel_RedirectsToPersonDetailPage(string page)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryRecord(personA)
                .WithAttributeSourcesSet()
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
    }

    [Theory]
    [InlineData("matches")]
    [InlineData("merge")]
    [InlineData("check-answers")]
    public async Task Post_OtherTrnNotSelected_RedirectsToEnterTrnPage(string page)
    {
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/manual-merge/enter-trn?{journeyInstance.GetUniqueIdQueryParameter()}");
    }

    [Theory]
    [InlineData("merge")]
    [InlineData("check-answers")]
    public async Task Post_PrimaryRecordNotSelected_RedirectsToMatches(string page)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/manual-merge/matches?{journeyInstance.GetUniqueIdQueryParameter()}");
    }

    [Theory]
    [InlineData("check-answers")]
    public async Task Post_PersonAttributeSourcesNotSet_RedirectsToMerge(string page)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryRecord(personA)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/manual-merge/merge?{journeyInstance.GetUniqueIdQueryParameter()}");
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
                .WithPrimaryRecord(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryRecordId(personB.PersonId)
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
                .WithPrimaryRecord(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryRecordId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("enter-trn", InductionStatus.InProgress)]
    [InlineData("enter-trn", InductionStatus.Passed)]
    [InlineData("enter-trn", InductionStatus.Failed)]
    [InlineData("matches", InductionStatus.InProgress)]
    [InlineData("matches", InductionStatus.Passed)]
    [InlineData("matches", InductionStatus.Failed)]
    [InlineData("merge", InductionStatus.InProgress)]
    [InlineData("merge", InductionStatus.Passed)]
    [InlineData("merge", InductionStatus.Failed)]
    [InlineData("check-answers", InductionStatus.InProgress)]
    [InlineData("check-answers", InductionStatus.Passed)]
    [InlineData("check-answers", InductionStatus.Failed)]
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
                .WithPrimaryRecord(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryRecordId(personB.PersonId)
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
                .WithPrimaryRecord(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryRecordId(personB.PersonId)
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
                .WithPrimaryRecord(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryRecordId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("matches", InductionStatus.InProgress)]
    [InlineData("matches", InductionStatus.Passed)]
    [InlineData("matches", InductionStatus.Failed)]
    [InlineData("merge", InductionStatus.InProgress)]
    [InlineData("merge", InductionStatus.Passed)]
    [InlineData("merge", InductionStatus.Failed)]
    [InlineData("check-answers", InductionStatus.InProgress)]
    [InlineData("check-answers", InductionStatus.Passed)]
    [InlineData("check-answers", InductionStatus.Failed)]
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
                .WithPrimaryRecord(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryRecordId(personB.PersonId)
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
                .WithPrimaryRecord(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryRecordId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var expectedRedirect = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedRedirect = $"{expectedRedirect}/manual-merge/{expectedPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
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
                .WithPrimaryRecord(personA)
                .WithAttributeSourcesSet()
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, page, journeyInstance, fromCheckAnswers: true))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryRecordId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var expectedRedirect = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedRedirect = $"{expectedRedirect}/manual-merge/{expectedPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
        }

        AssertEx.ResponseIsRedirectTo(response, expectedRedirect);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, string page, JourneyInstance<ManualMergeState>? journeyInstance = null, bool? fromCheckAnswers = null) =>
        $"/persons/{person.PersonId}/manual-merge/{page}?{journeyInstance?.GetUniqueIdQueryParameter()}{(fromCheckAnswers is bool f ? $"&fromCheckAnswers={f}" : "")}";

    private Task<JourneyInstance<ManualMergeState>> CreateJourneyInstanceAsync(Guid personId, ManualMergeState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.ManualMergePerson,
            state ?? new ManualMergeState(),
            new KeyValuePair<string, object>("personId", personId));
}
