using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditCompletedDateTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(InductionStatus.None)]
    [InlineData(InductionStatus.Exempt)]
    [InlineData(InductionStatus.RequiredToComplete)]
    [InlineData(InductionStatus.InProgress)]
    public async Task Get_WithInvalidJourneyState_InductionStatus_RedirectToStart(InductionStatus inductionStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/date-completed?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithInvalidJourneyState_StartDate_RedirectToStart()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(InductionStatus.Passed, InductionJourneyPage.StartDate)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/date-completed?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithCompletedDate_ShowsDate()
    {
        // Arrange
        var dateValid = Clock.Today;
        var inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithStartDate(dateValid.AddYears(-2))
                .WithCompletedDate(dateValid)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/date-completed?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var CompletedDate = doc.QuerySelectorAll<IHtmlInputElement>("[type=text]");
        Assert.Equal(dateValid.Day.ToString(), CompletedDate.ElementAt(0).Value);
        Assert.Equal(dateValid.Month.ToString(), CompletedDate.ElementAt(1).Value);
        Assert.Equal(dateValid.Year.ToString(), CompletedDate.ElementAt(2).Value);
    }

    [Fact]
    public async Task Post_SetValidCompletedDate_PersistsCompletedDate()
    {
        // Arrange
        var dateValid = Clock.Today;
        var inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithStartDate(Clock.Today.AddDays(-1))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/date-completed?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder().WithCompletedDate(dateValid).BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(dateValid, journeyInstance.State.CompletedDate);
    }

    [Fact]
    public async Task Post_NoCompletedDateIsEntered_ReturnsError()
    {
        // Arrange
        var inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithStartDate(Clock.Today.AddDays(-1))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/date-completed?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "CompletedDate", "Enter an induction completed date");
    }

    [Fact]
    public async Task Post_CompletedDateIsInTheFuture_ReturnsError()
    {
        // Arrange
        var dateTomorrow = Clock.Today.AddDays(1);
        var inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithStartDate(Clock.Today.AddDays(-1))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/date-completed?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder().WithCompletedDate(dateTomorrow).BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "CompletedDate", "The induction completed date cannot be in the future");
    }

    [Fact]
    public async Task Post_CompletedDateIsBeforeStartDate_ReturnsError()
    {
        // Arrange
        var completedDate = Clock.Today.AddDays(-1);
        var startDate = completedDate.AddDays(1);
        var inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithStartDate(startDate)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/date-completed?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder().WithCompletedDate(completedDate).BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "CompletedDate", "The induction completed date cannot be before the induction start date");
    }

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditInduction,
            state ?? new EditInductionState(),
            new KeyValuePair<string, object>("personId", personId));
}
