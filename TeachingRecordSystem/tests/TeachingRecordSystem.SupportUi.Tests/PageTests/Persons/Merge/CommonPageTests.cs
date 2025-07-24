using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Merge;

[Collection(nameof(DisableParallelization))]
public class CommonPageTests : TestBase
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
    [InlineData("compare-matching-records")]
    public async Task Get_FeatureFlagDisabled_ReturnsNotFound(string page)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);

        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, page, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);

        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
    }

    [Theory]
    [InlineData("enter-trn", null)]
    [InlineData("compare-matching-records", "enter-trn")]
    public async Task Get_BacklinkLinksToExpected(string page, string? expectedPage)
    {
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
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
    [InlineData("enter-trn")]
    [InlineData("compare-matching-records")]
    public async Task Get_ContinueAndCancelButtons_ExistOnPage(string page)
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
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
            b => Assert.Equal("Continue", b.TrimmedText()),
            b => Assert.Equal("Cancel and return to record", b.TrimmedText()));
    }

    [Theory]
    [InlineData("enter-trn")]
    [InlineData("compare-matching-records")]
    public async Task Post_Cancel_RedirectsToPersonDetailPage(string page)
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
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

    private string GetRequestPath(TestData.CreatePersonResult person, string page, JourneyInstance<MergeState>? journeyInstance = null) =>
        $"/persons/{person.PersonId}/merge/{page}?{journeyInstance?.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<MergeState>> CreateJourneyInstanceAsync(Guid personId, MergeState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.MergePerson,
            state ?? new MergeState(),
            new KeyValuePair<string, object>("personId", personId));
}
