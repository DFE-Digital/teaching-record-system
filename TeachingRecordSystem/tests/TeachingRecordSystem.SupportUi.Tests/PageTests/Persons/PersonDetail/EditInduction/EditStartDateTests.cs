using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditStartDateTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    [Arguments(InductionStatus.None)]
    [Arguments(InductionStatus.Exempt)]
    [Arguments(InductionStatus.RequiredToComplete)]
    public async Task Get_WithInvalidJourneyState_RedirectToStart(InductionStatus inductionStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Get_WithStartDate_ShowsDate()
    {
        // Arrange
        var dateValid = Clock.Today;
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithStartDate(dateValid)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var startDate = doc.QuerySelectorAll<IHtmlInputElement>("[type=text]");
        Assert.Equal(dateValid.Day.ToString(), startDate.ElementAt(0).Value);
        Assert.Equal(dateValid.Month.ToString(), startDate.ElementAt(1).Value);
        Assert.Equal(dateValid.Year.ToString(), startDate.ElementAt(2).Value);
    }

    [Test]
    public async Task Post_SetValidStartDate_PersistsStartDate()
    {
        // Arrange
        var dateValid = Clock.Today;
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder().WithStartDate(dateValid).BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(dateValid, journeyInstance.State.StartDate);
    }

    [Test]
    public async Task Post_NoStartDateIsEntered_ReturnsError()
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StartDate", "Enter an induction start date");
    }

    [Test]
    public async Task Post_StartDateIsInTheFuture_ReturnsError()
    {
        // Arrange
        var dateTomorrow = Clock.Today.AddDays(1);
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder().WithStartDate(dateTomorrow).BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StartDate", "The induction start date cannot be in the future");
    }

    [Test]
    public async Task Post_StartDateIsTooEarly_ReturnsError()
    {
        // Arrange
        var dateTooEarly = new DateOnly(1999, 5, 6);
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder().WithStartDate(dateTooEarly).BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StartDate", "The induction start date cannot be before 7 May 1999");
    }

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditInduction,
            state ?? new EditInductionState(),
            new KeyValuePair<string, object>("personId", personId));
}
