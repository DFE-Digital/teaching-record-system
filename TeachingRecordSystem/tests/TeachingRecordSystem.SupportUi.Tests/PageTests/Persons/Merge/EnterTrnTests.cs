using TeachingRecordSystem.SupportUi.Pages.Persons.Merge;
using Xunit.DependencyInjection;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Merge;

[Collection(nameof(DisableParallelization))]
public class EnterTrnTests : TestBase
{
    public EnterTrnTests(HostFixture hostFixture) : base(hostFixture)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
    }

    public override void Dispose()
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);
        base.Dispose();
    }

    [Fact]
    public async Task Get_PopulatesThisTrnFromPersonRecord()
    {
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new MergeStateBuilder()
                .WithInitializedState()
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var thisTrn = doc.GetElementByTestId("this-trn");

        Assert.NotNull(thisTrn);
        Assert.Equal(person.Trn, thisTrn.TrimmedText());
    }

    [Fact]
    public async Task Post_OtherTrnMissing_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new MergeStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EnterTrnModel.OtherTrn), "Enter a TRN");
    }

    [Theory]
    [InlineData("A234567")]
    [InlineData("XYZ")]
    public async Task Post_OtherTrnNotNumeric_ShowsPageError(string trn)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new MergeStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EnterTrnModel.OtherTrn), "TRN must be a number");
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("123456")]
    public async Task Post_OtherTrnNot7DigitsLong_ShowsPageError(string trn)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new MergeStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EnterTrnModel.OtherTrn), "TRN must be 7 digits long");
    }

    [Fact]
    public async Task Post_OtherTrnSameAsThisTrn_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new MergeStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(person.Trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EnterTrnModel.OtherTrn), "TRN must be for a different record");
    }

    [Fact]
    public async Task Post_OtherTrnDoesNotBelongToPerson_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var newTrn = await TestData.GenerateTrnAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new MergeStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(newTrn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EnterTrnModel.OtherTrn), "No record found with that TRN");
    }

    [Fact]
    public async Task Post_OtherTrnBelongsToDeactivatedPerson_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var otherPerson = await TestData.CreatePersonAsync(p => p.WithTrn());

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(otherPerson.Person);
            otherPerson.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new MergeStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(otherPerson.Trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EnterTrnModel.OtherTrn), "The TRN you entered belongs to a deactivated record");
    }

    [Fact]
    public async Task Post_PersistsDetailsAndRedirectsToNextPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var otherPerson = await TestData.CreatePersonAsync(p => p.WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new MergeStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(otherPerson.Trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(otherPerson.Trn, journeyInstance.State.OtherTrn);

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}/merge/compare-matching-records?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, JourneyInstance<MergeState>? journeyInstance = null) =>
        $"/persons/{person.PersonId}/merge/enter-trn?{journeyInstance?.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<MergeState>> CreateJourneyInstanceAsync(Guid personId, MergeState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.MergePerson,
            state ?? new MergeState(),
            new KeyValuePair<string, object>("personId", personId));

}
