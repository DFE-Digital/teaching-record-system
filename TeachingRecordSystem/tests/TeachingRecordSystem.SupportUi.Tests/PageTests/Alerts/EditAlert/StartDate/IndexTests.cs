using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.StartDate;

public class IndexTests(HostFixture hostFixture) : StartDateTestBase(hostFixture)
{
    private const string ThisStep = JourneySteps.Index;
    private const string PreviousStep = JourneySteps.New;

    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var databaseEndDate = new DateOnly(2022, 11, 6);
        var person = await TestData.CreatePerson(p => p.WithAlert(a => a.WithStartDate(databaseStartDate).WithEndDate(databaseEndDate)));
        var alert = person.Alerts.Single();
        var journeyInstance = await CreateEmptyJourneyInstance(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithUninitializedJourneyState_PopulatesModelFromDatabase()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var person = await TestData.CreatePerson(p => p.WithAlert(a => a.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alert = person.Alerts.Single();
        var journeyInstance = await CreateEmptyJourneyInstance(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal($"{databaseStartDate:%d}", doc.GetElementById("StartDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{databaseStartDate:%M}", doc.GetElementById("StartDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{databaseStartDate:yyyy}", doc.GetElementById("StartDate.Year")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Get_ValidRequestWithInitializedJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var journeyStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(p => p.WithAlert(a => a.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alert = person.Alerts.Single();
        var journeyInstance = await CreateJourneyInstanceForCompletedStep(ThisStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal($"{journeyStartDate:%d}", doc.GetElementById("StartDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{journeyStartDate:%M}", doc.GetElementById("StartDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{journeyStartDate:yyyy}", doc.GetElementById("StartDate.Year")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = GetPostContent(newStartDate: TestData.GenerateDate())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var databaseEndDate = new DateOnly(2022, 11, 6);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(databaseEndDate)));
        var alert = person.Alerts.Single();
        var journeyInstance = await CreateEmptyJourneyInstance(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = GetPostContent(newStartDate: databaseStartDate.AddDays(1))
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_NoStartDateIsEntered_ReturnsError()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alert = person.Alerts.Single();
        var journeyInstance = await CreateJourneyInstanceForCompletedStep(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = GetPostContent(newStartDate: null)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "StartDate", "Enter a start date");
    }

    [Fact]
    public async Task Post_StartDateIsInTheFuture_ReturnsError()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var futureDate = Clock.Today.AddDays(2);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alert = person.Alerts.Single();
        var journeyInstance = await CreateJourneyInstanceForCompletedStep(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = GetPostContent(newStartDate: futureDate)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "StartDate", "Start date cannot be in the future");
    }

    [Fact]
    public async Task Post_StartDateIsUnchanged_ReturnsError()
    {
        // Arrange
        var databaseStartDate = Clock.Today;
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alert = person.Alerts.Single();
        var journeyInstance = await CreateJourneyInstanceForCompletedStep(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = GetPostContent(newStartDate: databaseStartDate)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "StartDate", "Enter a different start date");
    }

    [Fact]
    public async Task Post_ValidStartDateIsEntered_UpdatesStateAndRedirectsToChangeReasonPage()
    {
        // Arrange
        var databaseStartDate = Clock.Today.AddDays(-20);
        var newStartDate = Clock.Today.AddDays(-18);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alert = person.Alerts.Single();
        var journeyInstance = await CreateJourneyInstanceForCompletedStep(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = GetPostContent(newStartDate: newStartDate)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/start-date/change-reason", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(newStartDate, journeyInstance.State.StartDate);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(null)));
        var alertId = person.Alerts.Single().AlertId;
        var alert = person.Alerts.Single();
        var journeyInstance = await CreateJourneyInstanceForCompletedStep(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private static FormUrlEncodedContentBuilder GetPostContent(DateOnly? newStartDate)
    {
        var builder = new FormUrlEncodedContentBuilder();

        if (newStartDate is DateOnly date)
        {
            builder.Add("StartDate.Day", date.Day);
            builder.Add("StartDate.Month", date.Month);
            builder.Add("StartDate.Year", date.Year);
        }

        return builder;
    }
}
