using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditCompletedDateTests(HostFixture hostFixture) : EditInductionTestBase(hostFixture)
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
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });

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
            new EditInductionState
            {
                InductionStatus = InductionStatus.Passed,
                CurrentInductionStatus = InductionStatus.Passed
            },
            StartPage.StartDate);

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
        var dateValid = TimeProvider.Today;
        var inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = dateValid.AddYears(-2),
                CompletedDate = dateValid
            });

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
        var dateValid = TimeProvider.Today;
        var inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddDays(-1)
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/date-completed?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder().WithCompletedDate(dateValid).BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        var state = GetJourneyInstanceState(journeyInstance)!;
        Assert.Equal(dateValid, state.CompletedDate);
    }

    [Fact]
    public async Task Post_NoCompletedDateIsEntered_ReturnsError()
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
                StartDate = TimeProvider.Today.AddDays(-1)
            });

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
        var dateTomorrow = TimeProvider.Today.AddDays(1);
        var inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddDays(-1)
            });

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
        var completedDate = TimeProvider.Today.AddDays(-1);
        var startDate = completedDate.AddDays(1);
        var inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = startDate
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/date-completed?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder().WithCompletedDate(completedDate).BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "CompletedDate", "The induction completed date cannot be before the induction start date");
    }

    [Theory]
    [InlineData(InductionStatus.Failed, "edit-induction/reason")]
    [InlineData(InductionStatus.FailedInWales, "edit-induction/reason")]
    [InlineData(InductionStatus.Passed, "edit-induction/reason")]
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/date-completed?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            $"/persons/{person.PersonId}/edit-induction/date-completed?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            $"/persons/{person.PersonId}/edit-induction/date-completed?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        });

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Theory]
    [InlineData(StartPage.CompletedDate, InductionStatus.Failed, "induction")]
    [InlineData(StartPage.Status, InductionStatus.Failed, "edit-induction/start-date")]
    [InlineData(StartPage.StartDate, InductionStatus.Failed, "edit-induction/start-date")]
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/date-completed?{journeyInstance.GetUniqueIdQueryParameter()}");

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
            $"/persons/{person.PersonId}/edit-induction/date-completed?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
            $"/persons/{person.PersonId}/edit-induction/date-completed?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}")
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
}
