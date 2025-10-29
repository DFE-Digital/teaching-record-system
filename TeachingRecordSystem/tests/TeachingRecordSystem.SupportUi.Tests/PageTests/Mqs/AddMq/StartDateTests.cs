using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.AddMq;

public class StartDateTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(personId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/start-date?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_SpecialismMissingFromState_RedirectsToSpecialism()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state => state.Specialism = null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/add/specialism?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Get_WithPersonIdForValidPerson_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var startDate = new DateOnly(2021, 10, 5);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            state => state.StartDate = startDate);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal($"{startDate:%d}", doc.GetElementById("StartDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{startDate:%M}", doc.GetElementById("StartDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{startDate:yyyy}", doc.GetElementById("StartDate.Year")?.GetAttribute("value"));
    }

    [Test]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var startDate = new DateOnly(2021, 11, 5);

        var journeyInstance = await CreateJourneyInstanceAsync(personId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/start-date?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "StartDate.Day", $"{startDate:%d}" },
                { "StartDate.Month", $"{startDate:%M}" },
                { "StartDate.Year", $"{startDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_SpecialismMissingFromState_RedirectToSpecialism()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var startDate = new DateOnly(2021, 11, 5);

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state => state.Specialism = null);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "StartDate.Day", $"{startDate:%d}" },
                { "StartDate.Month", $"{startDate:%M}" },
                { "StartDate.Year", $"{startDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/add/specialism?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Post_WhenNoStartDateIsEntered_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StartDate", "Enter a start date");
    }

    [Test]
    [Arguments(0)]
    [Arguments(1)]
    public async Task Post_StartDateIsAfterOrEqualToEndDate_RendersError(int daysAfter)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var startDate = new DateOnly(2021, 11, 5);
        var endDate = startDate.AddDays(-daysAfter);

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state => state.EndDate = endDate);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "StartDate.Day", $"{startDate:%d}" },
                { "StartDate.Month", $"{startDate:%M}" },
                { "StartDate.Year", $"{startDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StartDate", "Start date must be after end date");
    }

    [Test]
    public async Task Post_WhenStartDateIsEntered_RedirectsToResultPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var startDate = new DateOnly(2021, 11, 5);

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "StartDate.Day", $"{startDate:%d}" },
                { "StartDate.Month", $"{startDate:%M}" },
                { "StartDate.Year", $"{startDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Test]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(httpMethod, $"/mqs/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private Task<JourneyInstance<AddMqState>> CreateJourneyInstanceAsync(Guid personId, Action<AddMqState>? configureState = null)
    {
        var state = new AddMqState
        {
            ProviderId = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham").MandatoryQualificationProviderId,
            Specialism = MandatoryQualificationSpecialism.Visual
        };
        configureState?.Invoke(state);

        return CreateJourneyInstance(
            JourneyNames.AddMq,
            state,
            new KeyValuePair<string, object>("personId", personId));
    }
}
