using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class CommonPageTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData("edit-induction/status", InductionStatus.Exempt, "edit-induction/exemption-reasons")]
    [InlineData("edit-induction/status", InductionStatus.InProgress, "edit-induction/start-date")]
    [InlineData("edit-induction/status", InductionStatus.Failed, "edit-induction/start-date")]
    [InlineData("edit-induction/status", InductionStatus.FailedInWales, "edit-induction/start-date")]
    [InlineData("edit-induction/status", InductionStatus.Passed, "edit-induction/start-date")]
    [InlineData("edit-induction/status", InductionStatus.RequiredToComplete, "edit-induction/change-reason")]
    [InlineData("edit-induction/exemption-reasons", InductionStatus.Exempt, "edit-induction/change-reason")]
    [InlineData("edit-induction/start-date", InductionStatus.InProgress, "edit-induction/change-reason")]
    [InlineData("edit-induction/start-date", InductionStatus.Failed, "edit-induction/date-completed")]
    [InlineData("edit-induction/start-date", InductionStatus.FailedInWales, "edit-induction/date-completed")]
    [InlineData("edit-induction/start-date", InductionStatus.Passed, "edit-induction/date-completed")]
    [InlineData("edit-induction/date-completed", InductionStatus.Failed, "edit-induction/change-reason")]
    [InlineData("edit-induction/date-completed", InductionStatus.FailedInWales, "edit-induction/change-reason")]
    [InlineData("edit-induction/date-completed", InductionStatus.Passed, "edit-induction/change-reason")]
    [InlineData("edit-induction/change-reason", InductionStatus.Exempt, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.InProgress, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.Failed, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.FailedInWales, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.Passed, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.RequiredToComplete, "edit-induction/check-answers")]
    [InlineData("edit-induction/check-answers", InductionStatus.RequiredToComplete, "induction")]
    public async Task Post_RedirectsToExpectedPage(string fromPage, InductionStatus inductionStatus, string expectedNextPageUrl)
    {
        // Arrange
        var exemptionReasonIds = (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
            .Select(e => e.InductionExemptionReasonId)
            .RandomSelection(1)
            .ToArray();
        var person = await TestData.CreatePersonAsync(
            p => p
                .WithQts()
                .WithInductionStatus(i => i
                    .WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitialisedState(inductionStatus, InductionJourneyPage.Status)
                .Create());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(
                new EditInductionPostRequestBuilder()
                    .WithInductionStatus(inductionStatus)
                    .WithExemptionReasonIds(exemptionReasonIds)
                    .WithStartDate(Clock.Today.AddDays(-1))
                    .WithCompletedDate(Clock.Today)
                    .WithChangeReason(InductionChangeReasonOption.IncompleteDetails)
                    .WithChangeReasonDetailSelections(false)
                    .WithNoFileUploadSelection()
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = expectedNextPageUrl == "induction"
            ? $"/persons/{person.PersonId}/{expectedNextPageUrl}"
            : $"/persons/{person.PersonId}/{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Theory]
    [InlineData("edit-induction/status", InductionStatus.InProgress)]
    [InlineData("edit-induction/exemption-reasons", InductionStatus.Exempt)]
    [InlineData("edit-induction/start-date", InductionStatus.Passed)]
    [InlineData("edit-induction/date-completed", InductionStatus.Passed)]
    [InlineData("edit-induction/change-reason", InductionStatus.InProgress)]
    [InlineData("edit-induction/check-answers", InductionStatus.InProgress)]
    public async Task Cancel_RedirectsToExpectedPage(string fromPage, InductionStatus inductionStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(s => s.
                WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitialisedState(inductionStatus, InductionJourneyPage.Status)
                .Create());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, cancelButton!.FormAction);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        var location = redirectResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}/induction", location);
    }

    [Theory]
    [InlineData("edit-induction/status", InductionJourneyPage.Status)]
    [InlineData("edit-induction/exemption-reasons", InductionJourneyPage.ExemptionReason)]
    [InlineData("edit-induction/start-date", InductionJourneyPage.StartDate)]
    [InlineData("edit-induction/date-completed", InductionJourneyPage.CompletedDate)]
    public async Task Post_PersistsStartPageInfo(string page, InductionJourneyPage pageName)
    {
        // Arrange
        InductionStatus inductionStatus = InductionStatus.Passed;
        var exemptionReasonIds = (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
            .Select(e => e.InductionExemptionReasonId)
            .RandomSelection(1)
            .ToArray();
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .Create());
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/{page}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new EditInductionPostRequestBuilder()
                .WithInductionStatus(inductionStatus)
                .WithStartDate(Clock.Today.AddDays(-1))
                .WithCompletedDate(Clock.Today)
                .WithExemptionReasonIds(exemptionReasonIds)
                .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(pageName, journeyInstance.State.JourneyStartPage);
    }

    [Theory]
    [InlineData(InductionJourneyPage.Status, "edit-induction/status", InductionStatus.Exempt, "induction")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/status", InductionStatus.RequiredToComplete, "induction")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/exemption-reasons", InductionStatus.Exempt, "edit-induction/status")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/start-date", InductionStatus.InProgress, "edit-induction/status")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/start-date", InductionStatus.InProgress, "induction")]
    [InlineData(InductionJourneyPage.CompletedDate, "edit-induction/date-completed", InductionStatus.Failed, "induction")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/date-completed", InductionStatus.Failed, "edit-induction/start-date")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/date-completed", InductionStatus.Failed, "edit-induction/start-date")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/change-reason", InductionStatus.Exempt, "edit-induction/exemption-reasons")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/change-reason", InductionStatus.InProgress, "edit-induction/start-date")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/change-reason", InductionStatus.Failed, "edit-induction/date-completed")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/change-reason", InductionStatus.FailedInWales, "edit-induction/date-completed")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/change-reason", InductionStatus.Passed, "edit-induction/date-completed")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/change-reason", InductionStatus.RequiredToComplete, "edit-induction/status")]
    public async Task SubjectToStartPage_BacklinkContainsExpected(InductionJourneyPage startPage, string fromPage, InductionStatus inductionStatus, string expectedBackPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitialisedState(inductionStatus, startPage)
                .Create());
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}/{expectedBackPage}", backlink!.Href);
    }

    [Theory]
    [InlineData(InductionJourneyPage.Status, "edit-induction/status", InductionStatus.Exempt, "check-answers")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/start-date", InductionStatus.Passed, "check-answers")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/date-completed", InductionStatus.Passed, "check-answers")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/change-reason", InductionStatus.Passed, "check-answers")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/exemption-reasons", InductionStatus.Passed, "check-answers")]
    public async Task FromCheckYourAnswersPage_BacklinkContainsExpected(InductionJourneyPage startPage, string fromPage, InductionStatus inductionStatus, string expectedBackPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitialisedState(inductionStatus, startPage)
                .WithStartDate(new DateOnly(2000, 2, 2))
                .WithCompletedDate(new DateOnly(2002, 2, 2))
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .Create());
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/{fromPage}?FromCheckAnswers={JourneyFromCyaPage.Cya.ToString()}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}/edit-induction/{expectedBackPage}", backlink!.Href);
    }

    [Fact]
    public async Task CompletedDate_FromStartDate_FromCya_BacklinkContainsExpected()
    {
        // Arrange
        var fromPage = "edit-induction/date-completed";
        var inductionStatus = InductionStatus.Passed;
        var expectedBackPage = "edit-induction/start-date";
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitialisedState(inductionStatus, InductionJourneyPage.StartDate)
                .WithStartDate(new DateOnly(2000, 2, 2))
                .WithCompletedDate(new DateOnly(2002, 2, 2))
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .Create());
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/{fromPage}?FromCheckAnswers={JourneyFromCyaPage.CyaToStartDate.ToString()}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}/{expectedBackPage}", backlink!.Href);
    }

    [Theory]
    [InlineData("edit-induction/status", InductionStatus.Exempt, "edit-induction/exemption-reasons")]
    [InlineData("edit-induction/start-date", InductionStatus.Passed, "edit-induction/check-answers")]
    [InlineData("edit-induction/date-completed", InductionStatus.Passed, "edit-induction/check-answers")]
    [InlineData("edit-induction/exemption-reason", InductionStatus.Exempt, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.Passed, "edit-induction/check-answers")]
    public async Task FromCya_ToPage_Post_RedirectsToExpectedPage(string page, InductionStatus inductionStatus, string expectedNextPageUrl)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(
            p => p
                .WithQts()
                .WithInductionStatus(i => i
                    .WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
               .WithInitialisedState(inductionStatus, InductionJourneyPage.Status)
               .WithStartDate(new DateOnly(2000, 2, 2))
               .WithCompletedDate(new DateOnly(2002, 2, 2))
               .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
               .Create());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/{page}?FromCheckAnswers={JourneyFromCyaPage.Cya.ToString()}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(
                new EditInductionPostRequestBuilder()
                    .WithInductionStatus(inductionStatus)
                    .WithStartDate(Clock.Today.AddDays(-1))
                    .WithCompletedDate(Clock.Today)
                    .WithChangeReason(InductionChangeReasonOption.IncompleteDetails)
                    .WithChangeReasonDetailSelections(false)
                    .WithNoFileUploadSelection()
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}/{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Theory]
    [InlineData(InductionStatus.Passed, "2000-02-02", "2002-02-02", "check-answers")]
    [InlineData(InductionStatus.Passed, "2003-02-02", "2002-02-02", "date-completed")]
    public async Task FromCya_ToStartDate_Post_RedirectsToExpectedPage(InductionStatus inductionStatus, string startDateString, string completedDateString, string expectedNextPageUrl)
    {
        // Arrange
        var startDate = DateOnly.Parse(startDateString);
        var completedDate = DateOnly.Parse(completedDateString);
        var fromPage = "edit-induction/start-date";
        var person = await TestData.CreatePersonAsync(
            p => p
                .WithQts()
                .WithInductionStatus(i => i
                    .WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitialisedState(inductionStatus, InductionJourneyPage.Status)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .Create());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/{fromPage}?FromCheckAnswers={JourneyFromCyaPage.Cya.ToString()}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(
                new EditInductionPostRequestBuilder()
                    .WithInductionStatus(inductionStatus)
                    .WithStartDate(startDate)
                    .WithCompletedDate(completedDate)
                    .WithChangeReason(InductionChangeReasonOption.IncompleteDetails)
                    .WithChangeReasonDetailSelections(false)
                    .WithNoFileUploadSelection()
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        //var expectedUrl = $"/persons/{person.PersonId}/edit-induction/{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}"; // TODO - querystring for CYA_start
        var expectedUrl = $"/persons/{person.PersonId}/edit-induction/{expectedNextPageUrl}";
        Assert.Contains(expectedUrl, location);
    }

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditInduction,
            state ?? new EditInductionState(),
            new KeyValuePair<string, object>("personId", personId));
}
