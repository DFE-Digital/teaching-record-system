using FormFlow;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.AddMq;

public class CheckAnswersTests : TestBase
{
    public CheckAnswersTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(personId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/check-answers?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithPersonIdForValidPersonReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5)));
        var mqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue("959"); // University of Leeds
        var specialism = await TestData.ReferenceDataCache.GetSpecialismByValue("Hearing");
        var startDate = new DateOnly(2021, 3, 1);
        var result = dfeta_qualification_dfeta_MQ_Status.Passed;
        DateOnly? endDate = new DateOnly(2021, 11, 5);
        var journeyInstance = await CreateJourneyInstance(
            person.ContactId,
            new AddMqState()
            {
                MqEstablishmentValue = mqEstablishment.dfeta_Value,
                SpecialismValue = specialism.dfeta_Value,
                StartDate = startDate,
                Result = result,
                EndDate = endDate
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(dfeta_qualification_dfeta_MQ_Status.InProgress)]
    [InlineData(dfeta_qualification_dfeta_MQ_Status.Failed)]
    [InlineData(dfeta_qualification_dfeta_MQ_Status.Passed)]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState(dfeta_qualification_dfeta_MQ_Status result)
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5)));
        var mqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue("959"); // University of Leeds
        var specialism = await TestData.ReferenceDataCache.GetSpecialismByValue("Hearing");
        var startDate = new DateOnly(2021, 3, 1);
        DateOnly? endDate = result == dfeta_qualification_dfeta_MQ_Status.Passed ? new DateOnly(2021, 11, 5) : null;
        var journeyInstance = await CreateJourneyInstance(
            person.ContactId,
            new AddMqState()
            {
                MqEstablishmentValue = mqEstablishment.dfeta_Value,
                SpecialismValue = specialism.dfeta_Value,
                StartDate = startDate,
                Result = result,
                EndDate = endDate
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocument();
        Assert.Equal(mqEstablishment.dfeta_name, doc.GetElementByTestId("provider")!.TextContent);
        Assert.Equal(specialism.dfeta_name, doc.GetElementByTestId("specialism")!.TextContent);
        Assert.Equal(startDate.ToString("d MMMM yyyy"), doc.GetElementByTestId("start-date")!.TextContent);
        Assert.Equal(result.ToString(), doc.GetElementByTestId("result")!.TextContent);
        if (result == dfeta_qualification_dfeta_MQ_Status.Passed)
        {
            Assert.Equal(endDate!.Value.ToString("d MMMM yyyy"), doc.GetElementByTestId("end-date")!.TextContent);
        }
        else
        {
            Assert.Equal("None", doc.GetElementByTestId("end-date")!.TextContent);
        }
    }

    [Fact]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(personId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/check-answers?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(dfeta_qualification_dfeta_MQ_Status.InProgress)]
    [InlineData(dfeta_qualification_dfeta_MQ_Status.Failed)]
    [InlineData(dfeta_qualification_dfeta_MQ_Status.Passed)]
    public async Task Post_Confirm_CompletesJourneyAndRedirectsWithFlashMessage(dfeta_qualification_dfeta_MQ_Status result)
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5)));
        var mqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue("959"); // University of Leeds
        var specialism = await TestData.ReferenceDataCache.GetSpecialismByValue("Hearing");
        var startDate = new DateOnly(2021, 3, 1);
        DateOnly? endDate = result == dfeta_qualification_dfeta_MQ_Status.Passed ? new DateOnly(2021, 11, 5) : null;
        var journeyInstance = await CreateJourneyInstance(
            person.ContactId,
            new AddMqState()
            {
                MqEstablishmentValue = mqEstablishment.dfeta_Value,
                SpecialismValue = specialism.dfeta_Value,
                StartDate = startDate,
                Result = result,
                EndDate = endDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Mandatory qualification added");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5)));
        var mqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue("959"); // University of Leeds
        var specialism = await TestData.ReferenceDataCache.GetSpecialismByValue("Hearing");
        var startDate = new DateOnly(2021, 3, 1);
        var result = dfeta_qualification_dfeta_MQ_Status.Passed;
        DateOnly? endDate = new DateOnly(2021, 11, 5);
        var journeyInstance = await CreateJourneyInstance(
            person.ContactId,
            new AddMqState()
            {
                MqEstablishmentValue = mqEstablishment.dfeta_Value,
                SpecialismValue = specialism.dfeta_Value,
                StartDate = startDate,
                Result = result,
                EndDate = endDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/check-answers/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private async Task<JourneyInstance<AddMqState>> CreateJourneyInstance(Guid personId, AddMqState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.AddMq,
            state ?? new AddMqState(),
            new KeyValuePair<string, object>("personId", personId));
}
