using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

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
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(person)
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
    public async Task Get_OtherTrnAlreadyEntered_ShowsOtherTrn()
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

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var otherTrn = doc.GetChildElementOfTestId<IHtmlInputElement>("other-trn", "input");
        Assert.NotNull(otherTrn);
        Assert.Equal(personB.Trn, otherTrn.Value);
    }

    [Fact]
    public async Task Post_OtherTrnMissing_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(person)
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
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(person)
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
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(person)
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
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(person)
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
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var newTrn = await TestData.GenerateTrnAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(person)
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
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

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
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EnterTrnModel.OtherTrn), "The TRN you entered belongs to a deactivated record");
    }

    [Fact]
    public async Task Post_PersonAIsDeactivated_ReturnsBadRequest()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(personA.Person);
            personA.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_PersonAHasOpenAlert_ReturnsBadRequest()
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithAlert(a => a.WithEndDate(null)));

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    public async Task Post_PersonAHasInvalidInductionStatus_ReturnsBadRequest(InductionStatus status)
    {
        // Arrange
        var personA = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn()
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var personB = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .Build());

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_PersistsDetailsAndRedirectsToNextPage()
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
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/merge/compare-matching-records?{journeyInstance.GetUniqueIdQueryParameter()}");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(personA.PersonId, journeyInstance.State.PersonAId);
        Assert.Equal(personA.Trn, journeyInstance.State.PersonATrn);
        Assert.Equal(personB.PersonId, journeyInstance.State.PersonBId);
        Assert.Equal(personB.Trn, journeyInstance.State.PersonBTrn);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, JourneyInstance<MergeState>? journeyInstance = null) =>
        $"/persons/{person.PersonId}/merge/enter-trn?{journeyInstance?.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<MergeState>> CreateJourneyInstanceAsync(Guid personId, MergeState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.MergePerson,
            state ?? new MergeState(),
            new KeyValuePair<string, object>("personId", personId));

}
