using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class CheckYourAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private const string ChangeReasonDetails = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";

    public static IEnumerable<object[]> GetInductionStatusData()
    {
        yield return
        [
            new EditInductionStateBuilder().WithInitializedState(InductionStatus.InProgress, InductionJourneyPage.Status).WithCompletedDate(DateOnly.Parse("2024-12-31")).WithReasonChoice(InductionChangeReasonOption.AnotherReason).Build()
        ];
        yield return
        [
            new EditInductionStateBuilder().WithInitializedState(InductionStatus.Passed, InductionJourneyPage.Status).WithStartDate(DateOnly.Parse("2024-12-31")).WithReasonChoice(InductionChangeReasonOption.AnotherReason).Build()
        ];
        yield return
        [
            new EditInductionStateBuilder().WithInitializedState(InductionStatus.RequiredToComplete, InductionJourneyPage.Status).WithStartDate(DateOnly.Parse("2024-12-31")).WithReasonChoice(InductionChangeReasonOption.AnotherReason).Build()
        ];
        yield return
        [
            new EditInductionStateBuilder().WithInitializedState(InductionStatus.InProgress, InductionJourneyPage.Status).WithStartDate(DateOnly.Parse("2024-12-31")).Build()
        ];
    }

    [Theory]
    [MemberData(nameof(GetInductionStatusData))]
    public async Task Get_WithInvalidJourneyState_RedirectToStart(EditInductionState editInductionState)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(InductionJourneyPage.Status, InductionStatus.InProgress, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Passed, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Failed, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Exempt, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.FailedInWales, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.RequiredToComplete, true)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.InProgress, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Passed, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Failed, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.FailedInWales, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Passed, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Failed, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.FailedInWales, false)]
    [InlineData(InductionJourneyPage.ExemptionReason, InductionStatus.Exempt, false)]
    public async Task Get_ShowsInductionStatus_AsExpected(InductionJourneyPage startPage, InductionStatus inductionStatus, bool showChangeLink)
    {
        // Arrange
        var labelContent = "Induction status";

        DateOnly? startDate = inductionStatus.RequiresStartDate() ? Clock.Today.AddYears(-2) : null;
        DateOnly? completedDate = inductionStatus.RequiresCompletedDate() ? Clock.Today : null;

        var exemptionReasonIds = inductionStatus is InductionStatus.Exempt
            ? (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
                .RandomSelection(1)
                .Select(r => r.InductionExemptionReasonId)
                .ToArray()
            : [];

        var editInductionState = new EditInductionStateBuilder()
            .WithInitializedState(inductionStatus, startPage)
            .WithExemptionReasonIds(exemptionReasonIds)
            .WithStartDate(startDate)
            .WithCompletedDate(completedDate)
            .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
            .WithReasonDetailsChoice(false)
            .WithFileUploadChoice(false)
            .Build();

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == labelContent);
        Assert.NotNull(label);
        var value = label.NextElementSibling;
        Assert.NotNull(value);
        Assert.Equal(inductionStatus.GetTitle(), value.TrimmedText());
        if (showChangeLink)
        {
            Assert.NotNull(value.NextElementSibling!.GetElementsByTagName("a").First());
        }
        else
        {
            Assert.Null(value.NextElementSibling);
        }
    }

    [Theory]
    [InlineData(InductionJourneyPage.Status, InductionStatus.InProgress, true, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Passed, true, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Failed, true, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.FailedInWales, true, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Exempt, false, false)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.RequiredToComplete, false, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.InProgress, true, true)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Passed, true, true)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Failed, true, true)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.FailedInWales, true, true)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Passed, true, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Failed, true, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.FailedInWales, true, false)]
    [InlineData(InductionJourneyPage.ExemptionReason, InductionStatus.Exempt, false, false)]
    public async Task Get_ShowsStartDate_AsExpected(InductionJourneyPage startPage, InductionStatus inductionStatus, bool showStartDateRow, bool showChangeLink)
    {
        // Arrange
        var labelContent = "Induction started on";

        DateOnly? startDate = inductionStatus.RequiresStartDate() ? Clock.Today.AddYears(-2) : null;
        DateOnly? completedDate = inductionStatus.RequiresCompletedDate() ? Clock.Today : null;

        var exemptionReasonIds = inductionStatus is InductionStatus.Exempt
            ? (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
            .RandomSelection(1)
            .Select(r => r.InductionExemptionReasonId)
            .ToArray()
            : [];

        var editInductionState = new EditInductionStateBuilder()
            .WithInitializedState(inductionStatus, startPage)
            .WithExemptionReasonIds(exemptionReasonIds)
            .WithStartDate(startDate)
            .WithCompletedDate(completedDate)
            .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
            .WithReasonDetailsChoice(false)
            .WithFileUploadChoice(false)
            .Build();

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        if (showStartDateRow)
        {
            var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == labelContent);
            Assert.NotNull(label.NextElementSibling);
            var value = label.NextElementSibling;
            Assert.Equal(startDate?.ToString(UiDefaults.DateOnlyDisplayFormat), value!.TrimmedText());
            if (showChangeLink)
            {
                Assert.NotNull(value.NextElementSibling!.GetElementsByTagName("a").First());
            }
            else
            {
                Assert.Null(value.NextElementSibling);
            }
        }
        else
        {
            Assert.DoesNotContain(doc.QuerySelectorAll(".govuk-summary-list__key"), e => e.TrimmedText() == labelContent);
        }
    }

    [Theory]
    [InlineData(InductionJourneyPage.Status, InductionStatus.InProgress, false)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Passed, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Failed, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.FailedInWales, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Exempt, false)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.RequiredToComplete, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.InProgress, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Passed, true)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Failed, true)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.FailedInWales, true)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Passed, true)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Failed, true)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.FailedInWales, true)]
    [InlineData(InductionJourneyPage.ExemptionReason, InductionStatus.Exempt, false)]
    public async Task Get_ShowsCompletedDate_AsExpected(InductionJourneyPage startPage, InductionStatus inductionStatus, bool ShowsCompletedDate)
    {
        // Arrange
        var labelContent = "Induction completed on";

        DateOnly? startDate = inductionStatus.RequiresStartDate() ? Clock.Today.AddYears(-2) : null;
        DateOnly? completedDate = inductionStatus.RequiresCompletedDate() ? Clock.Today : null;

        var exemptionReasonIds = inductionStatus is InductionStatus.Exempt
            ? (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
            .RandomSelection(1)
            .Select(r => r.InductionExemptionReasonId)
            .ToArray()
            : [];

        var editInductionState = new EditInductionStateBuilder()
            .WithInitializedState(inductionStatus, startPage)
            .WithExemptionReasonIds(exemptionReasonIds)
            .WithStartDate(startDate)
            .WithCompletedDate(completedDate)
            .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
            .WithReasonDetailsChoice(false)
            .WithFileUploadChoice(false)
            .Build();

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        if (ShowsCompletedDate)
        {
            var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == labelContent);
            Assert.NotNull(label);
            var value = label.NextElementSibling;
            Assert.Equal(completedDate?.ToString(UiDefaults.DateOnlyDisplayFormat), value!.TrimmedText());
        }
        else
        {
            Assert.DoesNotContain(doc.QuerySelectorAll(".govuk-summary-list__key"), e => e.TrimmedText() == labelContent);
        }
    }

    [Theory]
    [InlineData(InductionJourneyPage.Status, InductionStatus.InProgress, false)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Passed, false)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Failed, false)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.FailedInWales, false)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Exempt, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.RequiredToComplete, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.InProgress, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Passed, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Failed, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.FailedInWales, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Passed, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Failed, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.FailedInWales, false)]
    [InlineData(InductionJourneyPage.ExemptionReason, InductionStatus.Exempt, true)]
    public async Task Get_ShowsExemptionReason_AsExpected(InductionJourneyPage startPage, InductionStatus inductionStatus, bool ShowsExemptionReason)
    {
        // Arrange
        var labelContent = "Exemption reason";

        DateOnly? startDate = inductionStatus.RequiresStartDate() ? Clock.Today.AddYears(-2) : null;
        DateOnly? completedDate = inductionStatus.RequiresCompletedDate() ? Clock.Today : null;

        //var exemptionReasonIds = inductionStatus is InductionStatus.Exempt
        //    ? (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
        //    .RandomSelection(2)
        //    .Select(r => r.InductionExemptionReasonId)
        //    .ToArray()
        //    : [];
        var exemptionReasonIds = inductionStatus is InductionStatus.Exempt
            ? new Guid[] { InductionExemptionReason.ExemptDataLossOrErrorCriteriaId, InductionExemptionReason.ExemptId }
            : [];

        var expectedReasons = (await TestData.ReferenceDataCache
            .GetInductionExemptionReasonsAsync(activeOnly: true))
            .Where(r => exemptionReasonIds.Contains(r.InductionExemptionReasonId))
            .Select(r => r.Name)
            .OrderByDescending(r => r)
            .ToArray();

        var editInductionState = new EditInductionStateBuilder()
            .WithInitializedState(inductionStatus, startPage)
            .WithExemptionReasonIds(exemptionReasonIds)
            .WithStartDate(startDate)
            .WithCompletedDate(completedDate)
            .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
            .WithReasonDetailsChoice(false)
            .WithFileUploadChoice(false)
            .Build();

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        if (ShowsExemptionReason)
        {
            var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == labelContent);
            Assert.NotNull(label);
            var reasons = label.NextElementSibling!.QuerySelectorAll("li").Select(d => d.TrimmedText());
            Assert.NotEmpty(reasons);
            Assert.Equal(expectedReasons, reasons);
        }
        else
        {
            Assert.DoesNotContain(doc.QuerySelectorAll(".govuk-summary-list__key"), e => e.TrimmedText() == labelContent);
        }
    }

    [Fact]
    public async Task Cancel_RedirectsToExpectedPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(s => s.
                WithStatus(InductionStatus.RequiredToComplete)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(InductionStatus.RequiredToComplete, InductionJourneyPage.Status)
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .WithReasonDetailsChoice(false)
                .WithFileUploadChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

    [Fact]
    public async Task Get_ShowsChangeReason_AsExpected()
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;
        var startDate = Clock.Today.AddYears(-2);
        var completedDate = Clock.Today;
        var exemptionReasonIds = Array.Empty<Guid>();

        var editInductionState = new EditInductionStateBuilder()
            .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
            .WithExemptionReasonIds(exemptionReasonIds)
            .WithStartDate(startDate)
            .WithCompletedDate(completedDate)
            .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
            .WithReasonDetailsChoice(addDetails: true, ChangeReasonDetails)
            .WithFileUploadChoice(uploadFile: false)
            .Build();

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == "Reason for changing induction details");
        Assert.NotNull(label);
        var value = label.NextElementSibling;
        Assert.Equal(InductionChangeReasonOption.AnotherReason.GetDisplayName(), value!.TrimmedText());

        var labelDetails = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == "Reason details");
        Assert.NotNull(labelDetails);
        var valueDetails = labelDetails.NextElementSibling;
        Assert.Equal(ChangeReasonDetails, valueDetails!.TrimmedText());

        var labelFileUpload = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == "Do you have evidence to upload");
        Assert.NotNull(labelFileUpload);
        var valueFileUpload = labelFileUpload.NextElementSibling;
        Assert.Equal("Not provided", valueFileUpload!.TrimmedText());
    }

    [Fact]
    public async Task Post_InvalidCompletedDate_RedirectToCompletedDatePage()
    {
        // Arrange
        var inductionStatus = InductionStatus.RequiredToComplete;
        var startDate = Clock.Today;
        var completedDate = startDate.AddYears(-2);
        var exemptionReasonIds = Array.Empty<Guid>();

        var person = await TestData.CreatePersonAsync(
            p => p
                .WithQts()
                .WithInductionStatus(i => i
                    .WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .WithReasonDetailsChoice(addDetails: true, ChangeReasonDetails)
                .WithFileUploadChoice(uploadFile: false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;

        Assert.Equal($"/persons/{person.PersonId}/edit-induction/date-completed?fromCheckAnswers={JourneyFromCheckYourAnswersPage.CheckYourAnswers}&{journeyInstance.GetUniqueIdQueryParameter()}", location);
    }

    [Fact]
    public async Task Post_RedirectsToExpectedPage()
    {
        // Arrange
        var inductionStatus = InductionStatus.RequiredToComplete;
        var startDate = Clock.Today.AddYears(-2);
        var completedDate = Clock.Today;
        var exemptionReasonIds = (await TestData.ReferenceDataCache
            .GetInductionExemptionReasonsAsync(activeOnly: true))
            .RandomSelection(1)
            .Select(r => r.InductionExemptionReasonId)
            .ToArray();
        var person = await TestData.CreatePersonAsync(
            p => p
                .WithQts()
                .WithInductionStatus(i => i
                    .WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .WithReasonDetailsChoice(addDetails: true, ChangeReasonDetails)
                .WithFileUploadChoice(uploadFile: false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;

        Assert.Equal($"/persons/{person.PersonId}/induction", location);
    }

    [Fact]
    public async Task Post_Confirm_UpdatesPersonInductionCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;

        DateOnly? startDate = inductionStatus.RequiresStartDate() ? Clock.Today.AddYears(-2) : null;
        DateOnly? completedDate = inductionStatus.RequiresCompletedDate() ? Clock.Today : null;

        var exemptionReasonIds = Array.Empty<Guid>();

        var person = await TestData.CreatePersonAsync(
            p => p
                .WithQts()
                .WithInductionStatus(i => i
                    .WithStatus(InductionStatus.RequiredToComplete)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .WithReasonDetailsChoice(addDetails: true, ChangeReasonDetails)
                .WithFileUploadChoice(uploadFile: true)
                .Build());

        EventPublisher.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Induction details have been updated");

        await WithDbContext(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons.FirstOrDefaultAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(journeyInstance.State.InductionStatus, updatedPersonRecord!.InductionStatus);
            Assert.Equal(journeyInstance.State.StartDate, updatedPersonRecord!.InductionStartDate);
            Assert.Equal(journeyInstance.State.CompletedDate, updatedPersonRecord!.InductionCompletedDate);
            Assert.Equal(journeyInstance.State.ExemptionReasonIds, updatedPersonRecord!.InductionExemptionReasonIds);
        });

        var RaisedBy = GetCurrentUserId();

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualInductionUpdatedEvent = Assert.IsType<PersonInductionUpdatedEvent>(e);

            Assert.Equal(actualInductionUpdatedEvent.CreatedUtc, Clock.UtcNow);
            Assert.Equal(actualInductionUpdatedEvent.PersonId, person.PersonId);
            Assert.Equal(actualInductionUpdatedEvent.Induction.Status, journeyInstance.State.InductionStatus);
            Assert.Equal(actualInductionUpdatedEvent.Induction.StartDate, journeyInstance.State.StartDate);
            Assert.Equal(actualInductionUpdatedEvent.Induction.CompletedDate, journeyInstance.State.CompletedDate);
            Assert.Equal(actualInductionUpdatedEvent.Induction.ExemptionReasonIds, journeyInstance.State.ExemptionReasonIds!);
            Assert.Equal(actualInductionUpdatedEvent.ChangeReason, journeyInstance.State.ChangeReason!.GetDisplayName());
            Assert.Equal(actualInductionUpdatedEvent.ChangeReasonDetail, journeyInstance.State.ChangeReasonDetail);
            Assert.Equal(actualInductionUpdatedEvent.EvidenceFile!.FileId, journeyInstance.State.EvidenceFileId!.Value);
            Assert.Equal(actualInductionUpdatedEvent.EvidenceFile.Name, journeyInstance.State.EvidenceFileName);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }


    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditInduction,
            state ?? new EditInductionState(),
            new KeyValuePair<string, object>("personId", personId));
}
