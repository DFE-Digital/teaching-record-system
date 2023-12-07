using FormFlow;
using TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.AddMq;

public class ResultTests : TestBase
{
    public ResultTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(personId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/result?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5)));
        var result = Core.Dqt.Models.dfeta_qualification_dfeta_MQ_Status.Passed;
        var endDate = new DateOnly(2021, 11, 5);
        var journeyInstance = await CreateJourneyInstance(
            person.ContactId,
            new AddMqState()
            {
                Result = result,
                EndDate = endDate,
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/result?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocument();
        var resultOptions = doc.GetElementByTestId("result-options");
        var radioButtons = resultOptions!.GetElementsByTagName("input");
        var selectedResult = radioButtons.SingleOrDefault(r => r.HasAttribute("checked"));
        Assert.NotNull(selectedResult);
        Assert.Equal(result.ToString(), selectedResult.GetAttribute("value"));
        Assert.Equal($"{endDate:%d}", doc.GetElementById("EndDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{endDate:%M}", doc.GetElementById("EndDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{endDate:yyyy}", doc.GetElementById("EndDate.Year")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Get_WithPersonIdForValidPerson_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5)));
        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/result?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var result = Core.Dqt.Models.dfeta_qualification_dfeta_MQ_Status.Passed;
        var endDate = new DateOnly(2021, 11, 5);
        var journeyInstance = await CreateJourneyInstance(personId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/result?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Result", result.ToString() },
                { "EndDate.Day", $"{endDate:%d}" },
                { "EndDate.Month", $"{endDate:%M}" },
                { "EndDate.Year", $"{endDate:yyyy}" },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenResultIsNotSelectedIsEntered_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5)));
        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/result?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Result", "Select a result");
    }

    [Fact]
    public async Task Post_WhenResultIsPassedAndEndDateHasNotBeenEntered_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5)));
        var result = Core.Dqt.Models.dfeta_qualification_dfeta_MQ_Status.Passed;
        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/result?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Result", result.ToString() },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EndDate", "Enter an end date");
    }

    [Fact]
    public async Task Post_ValidRequest_RedirectsToResultPage()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5)));
        var result = Core.Dqt.Models.dfeta_qualification_dfeta_MQ_Status.Passed;
        var endDate = new DateOnly(2021, 11, 5);
        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/result?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Result", result.ToString() },
                { "EndDate.Day", $"{endDate:%d}" },
                { "EndDate.Month", $"{endDate:%M}" },
                { "EndDate.Year", $"{endDate:yyyy}" },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    private async Task<JourneyInstance<AddMqState>> CreateJourneyInstance(Guid personId, AddMqState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.AddMq,
            state ?? new AddMqState(),
            new KeyValuePair<string, object>("personId", personId));

}
