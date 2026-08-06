using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditStartDateTests(HostFixture hostFixture) : EditInductionTestBase(hostFixture)
{
    [Theory]
    [InlineData(InductionStatus.None)]
    [InlineData(InductionStatus.Exempt)]
    [InlineData(InductionStatus.RequiredToComplete)]
    public async Task Get_WithInvalidJourneyState_RedirectToStart(InductionStatus inductionStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithStartDate_ShowsDate()
    {
        // Arrange
        var dateValid = TimeProvider.Today;
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = dateValid
            });

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

    [Fact]
    public async Task Post_SetValidStartDate_PersistsStartDate()
    {
        // Arrange
        var dateValid = TimeProvider.Today;
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder().WithStartDate(dateValid).BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        var state = GetJourneyInstanceState(journeyInstance)!;
        Assert.Equal(dateValid, state.StartDate);
    }

    [Fact]
    public async Task Post_NoStartDateIsEntered_ReturnsError()
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StartDate", "Enter an induction start date");
    }

    [Fact]
    public async Task Post_StartDateIsInTheFuture_ReturnsError()
    {
        // Arrange
        var dateTomorrow = TimeProvider.Today.AddDays(1);
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder().WithStartDate(dateTomorrow).BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StartDate", "The induction start date cannot be in the future");
    }

    [Fact]
    public async Task Post_StartDateIsTooEarly_ReturnsError()
    {
        // Arrange
        var dateTooEarly = new DateOnly(1999, 5, 6);
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder().WithStartDate(dateTooEarly).BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StartDate", "The induction start date cannot be before 7 May 1999");
    }

    [Theory]
    [InlineData(InductionStatus.InProgress, "edit-induction/reason")]
    [InlineData(InductionStatus.Failed, "edit-induction/date-completed")]
    [InlineData(InductionStatus.FailedInWales, "edit-induction/date-completed")]
    [InlineData(InductionStatus.Passed, "edit-induction/date-completed")]
    public async Task Post_RedirectsToExpectedPage(InductionStatus inductionStatus, string expectedNextPageUrl)
    {
        // Arrange
        var exemptionReasonIds = new Guid[] { InductionExemptionReason.ExemptId };
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(i => i.WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddYears(-2)
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(inductionStatus)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(TimeProvider.Today.AddDays(-1))
                .WithCompletedDate(TimeProvider.Today)
                .WithChangeReason(PersonInductionChangeReason.IncompleteDetails)
                .WithProvideAdditionalInformation(false)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/persons/{person.PersonId}/{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToInduction()
    {
        // Arrange
        var inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(s => s.WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddYears(-2),
                CompletedDate = TimeProvider.Today,
                ChangeReason = PersonInductionChangeReason.AnotherReason,
                ProvideAdditionalInformation = true,
                AdditionalInformation = "Details",
                Evidence = CreateEvidence(false)
            });

        // Act
        // Cancelling is a field on the form rather than a handler of its own: a distinct URL would be
        // an invalid step for the journey.
        var response = await HttpClient.SendAsync(new HttpRequestMessage(
            HttpMethod.Post,
            $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        });

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/induction", response.Headers.Location?.OriginalString);
        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_Cancel_EvidenceFilePreviouslyUploaded_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var inductionStatus = InductionStatus.Passed;
        var evidenceFileId = Guid.NewGuid();
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(s => s.WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddYears(-2),
                CompletedDate = TimeProvider.Today,
                ChangeReason = PersonInductionChangeReason.AnotherReason,
                ProvideAdditionalInformation = true,
                AdditionalInformation = "Details",
                Evidence = CreateEvidence(true, evidenceFileId)
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(
            HttpMethod.Post,
            $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        });

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Theory]
    [InlineData(StartPage.Status, InductionStatus.InProgress, "edit-induction/status")]
    [InlineData(StartPage.StartDate, InductionStatus.InProgress, "induction")]
    public async Task Get_BackLinkContainsExpected(StartPage startPage, InductionStatus inductionStatus, string expectedBackPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddYears(-2),
                CompletedDate = TimeProvider.Today
            },
            startPage);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}/{expectedBackPage}", backlink!.Href);
    }

    [Fact]
    public async Task Get_FromCheckAnswers_BackLinkReturnsToCheckAnswers()
    {
        // Arrange
        var inductionStatus = InductionStatus.Passed;
        var exemptionReasonIds = new Guid[] { InductionExemptionReason.ExemptId };
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                ExemptionReasonIds = exemptionReasonIds,
                StartDate = new DateOnly(2000, 2, 2),
                CompletedDate = new DateOnly(2002, 2, 2),
                ChangeReason = PersonInductionChangeReason.AnotherReason
            });

        var checkAnswersUrl = $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/persons/{person.PersonId}/edit-induction/start-date?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}/edit-induction/check-answers", backlink!.Href);
    }

    [Theory]
    [InlineData(InductionStatus.Passed, "edit-induction/check-answers")]
    public async Task Post_FromCheckAnswers_RedirectsToExpectedPage(InductionStatus inductionStatus, string expectedNextPageUrl)
    {
        // Arrange
        var startDate = new DateOnly(2000, 2, 1);
        var completedDate = new DateOnly(2002, 2, 2);
        var exemptionReasonIds = new Guid[] { InductionExemptionReason.ExemptId };

        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(i => i.WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                ExemptionReasonIds = exemptionReasonIds,
                StartDate = startDate,
                CompletedDate = completedDate,
                ChangeReason = PersonInductionChangeReason.AnotherReason
            });

        var checkAnswersUrl = $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/persons/{person.PersonId}/edit-induction/start-date?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(inductionStatus)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithChangeReason(PersonInductionChangeReason.IncompleteDetails)
                .WithProvideAdditionalInformation(false)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/persons/{person.PersonId}/{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_FromCheckAnswers_LeavesTheOtherQuestionsReachable()
    {
        // Arrange
        var inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(s => s.WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = new DateOnly(2021, 1, 1),
                CompletedDate = new DateOnly(2022, 1, 1),
                ChangeReason = PersonInductionChangeReason.AnotherReason,
                ChangeReasonDetail = "A reason",
                ProvideAdditionalInformation = false,
                Evidence = CreateEvidence(false)
            });

        var checkAnswersUrl = $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

        // Act
        // Change the start date from check answers, leaving it as it was.
        var startDateResponse = await HttpClient.SendAsync(new HttpRequestMessage(
            HttpMethod.Post,
            $"/persons/{person.PersonId}/edit-induction/start-date?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithStartDate(new DateOnly(2021, 1, 1))
                .BuildFormUrlEncoded()
        });

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)startDateResponse.StatusCode);
        Assert.Equal(checkAnswersUrl, startDateResponse.Headers.Location?.OriginalString);

        // Going back to check answers must not push a step: that would truncate the path and take the
        // completed date question — which check answers still offers a Change link for — with it.
        var completedDateResponse = await HttpClient.GetAsync(
            $"/persons/{person.PersonId}/edit-induction/date-completed?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}");

        Assert.Equal(StatusCodes.Status200OK, (int)completedDateResponse.StatusCode);
    }

    [Fact]
    public async Task Post_StartDate_FromCheckAnswers_AfterCompletedDate_AsksForCompletedDateWithBackLinkToStartDate()
    {
        // Arrange
        var inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = new DateOnly(2000, 2, 2),
                CompletedDate = new DateOnly(2002, 2, 2),
                ChangeReason = PersonInductionChangeReason.AnotherReason,
                ProvideAdditionalInformation = false,
                Evidence = CreateEvidence(false)
            },
            StartPage.StartDate);

        var checkAnswersUrl = $"/persons/{person.PersonId}/edit-induction/check-answers";

        // Change the start date from check answers to one that falls after the completed date.
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/persons/{person.PersonId}/edit-induction/start-date?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithStartDate(new DateOnly(2003, 2, 2))
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        Assert.StartsWith($"/persons/{person.PersonId}/edit-induction/date-completed", location);

        // Answering it returns the user to check answers, which is where they came from.
        var completedDateResponse = await HttpClient.GetAsync(location);
        var document = await completedDateResponse.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}/edit-induction/check-answers", backlink!.Href);
    }

    [Theory]
    [InlineData(InductionStatus.Passed, "2000-02-01", "2002-02-02", "check-answers")] // Start date not within two years of completed date
    [InlineData(InductionStatus.Passed, "2003-02-02", "2002-02-02", "date-completed")] // Start date after completed date
    public async Task Post_FromCya_ToStartDate_Post_RedirectsToExpectedPage(InductionStatus inductionStatus, string startDateString, string completedDateString, string expectedNextPageUrl)
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
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = startDate,
                CompletedDate = completedDate,
                ChangeReason = PersonInductionChangeReason.AnotherReason
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/{fromPage}?returnUrl={Uri.EscapeDataString($"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(inductionStatus)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithChangeReason(PersonInductionChangeReason.IncompleteDetails)
                .WithProvideAdditionalInformation(false)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}/edit-induction/{expectedNextPageUrl}";
        Assert.Contains(expectedUrl, location);
    }
}
