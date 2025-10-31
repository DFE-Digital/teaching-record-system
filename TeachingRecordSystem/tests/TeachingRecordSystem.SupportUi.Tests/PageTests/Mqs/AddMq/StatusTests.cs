using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.AddMq;

public class StatusTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceAsync(personId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/status?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_StartDateMissingFromState_RedirectsToStartDate()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            state => state.StartDate = null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var status = MandatoryQualificationStatus.Passed;
        var endDate = new DateOnly(2021, 11, 5);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            state =>
            {
                state.Status = status;
                state.EndDate = endDate;
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var resultOptions = doc.GetElementByTestId("status-options");
        var radioButtons = resultOptions!.GetElementsByTagName("input");
        var selectedResult = radioButtons.SingleOrDefault(r => r.HasAttribute("checked"));
        Assert.NotNull(selectedResult);
        Assert.Equal(status.ToString(), selectedResult.GetAttribute("value"));
        Assert.Equal($"{endDate:%d}", doc.GetElementById("EndDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{endDate:%M}", doc.GetElementById("EndDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{endDate:yyyy}", doc.GetElementById("EndDate.Year")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Get_WithPersonIdForValidPerson_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var status = MandatoryQualificationStatus.Passed;
        var endDate = new DateOnly(2021, 11, 5);

        var journeyInstance = await CreateJourneyInstanceAsync(personId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/status?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Status", status.ToString() },
                { "EndDate.Day", $"{endDate:%d}" },
                { "EndDate.Month", $"{endDate:%M}" },
                { "EndDate.Year", $"{endDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_StartDateMissingFromState_RedirectsToStartDate()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var status = MandatoryQualificationStatus.Passed;
        var endDate = new DateOnly(2021, 11, 5);

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state => state.StartDate = null);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Status", status.ToString() },
                { "EndDate.Day", $"{endDate:%d}" },
                { "EndDate.Month", $"{endDate:%M}" },
                { "EndDate.Year", $"{endDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/add/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenResultIsNotSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Status", "Select a status");
    }

    [Fact]
    public async Task Post_WhenResultIsPassedAndEndDateHasNotBeenEntered_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var status = MandatoryQualificationStatus.Passed;

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Status", status.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EndDate", "Enter an end date");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Post_EndDateIsBeforeOrEqualToStartDate_RendersError(int daysBefore)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var status = MandatoryQualificationStatus.Passed;
        var endDate = new DateOnly(2021, 11, 5);
        var startDate = endDate.AddDays(daysBefore);

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state => state.StartDate = startDate);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Status", status.ToString() },
                { "EndDate.Day", $"{endDate:%d}" },
                { "EndDate.Month", $"{endDate:%M}" },
                { "EndDate.Year", $"{endDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EndDate", "End date must be after start date");
    }

    [Fact]
    public async Task Post_ValidRequest_RedirectsToResultPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var status = MandatoryQualificationStatus.Passed;
        var endDate = new DateOnly(2021, 11, 5);

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Status", status.ToString() },
                { "EndDate.Day", $"{endDate:%d}" },
                { "EndDate.Month", $"{endDate:%M}" },
                { "EndDate.Year", $"{endDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
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
        var status = MandatoryQualificationStatus.Passed;
        var endDate = new DateOnly(2021, 11, 5);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            state =>
            {
                state.Status = status;
                state.EndDate = endDate;
            });

        var request = new HttpRequestMessage(httpMethod, $"/mqs/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
            Specialism = MandatoryQualificationSpecialism.Visual,
            StartDate = new(2020, 9, 1)
        };
        configureState?.Invoke(state);

        return CreateJourneyInstance(
            JourneyNames.AddMq,
            state,
            new KeyValuePair<string, object>("personId", personId));
    }
}
