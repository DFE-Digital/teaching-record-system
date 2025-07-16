using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.Merge;
using Xunit.DependencyInjection;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Merge;

[Collection(nameof(DisableParallelization))]
public class CompareMatchingRecordsTests : TestBase
{
    public CompareMatchingRecordsTests(HostFixture hostFixture) : base(hostFixture)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
    }

    public override void Dispose()
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);
        base.Dispose();
    }

    [Fact]
    public async Task Get_BacklinkLinksToPersonDetails()
    {
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new MergeStateBuilder()
                .WithInitializedState()
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        Assert.Contains($"/persons/{person.PersonId}/merge/enter-trn", backlink.Href);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, JourneyInstance<MergeState>? journeyInstance = null) =>
        $"/persons/{person.PersonId}/merge/compare-matching-records?{journeyInstance?.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<MergeState>> CreateJourneyInstanceAsync(Guid personId, MergeState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.MergePerson,
            state ?? new MergeState(),
            new KeyValuePair<string, object>("personId", personId));
}
