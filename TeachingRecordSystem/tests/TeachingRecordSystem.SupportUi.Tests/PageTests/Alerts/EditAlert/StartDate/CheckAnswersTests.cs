using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.StartDate;

public class CheckAnswersTests : TestBase
{
    public CheckAnswersTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.AllAlertsWriter);
    }

    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var databaseStartDate = new DateOnly(2021, 10, 5);
        var journeyStartDate = new DateOnly(2021, 10, 6);
        var changeReason = AlertChangeStartDateReasonOption.AnotherReason;
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate)));
        var alertId = person.Alerts.Single().AlertId;

        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertStartDateState()
            {
                Initialized = true,
                CurrentStartDate = databaseStartDate,
                StartDate = journeyStartDate,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = false,
                ChangeReasonDetail = null,
                UploadEvidence = false,
                EvidenceFileId = null,
                EvidenceFileName = null
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/start-date", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_WithValidJourneyState_ReturnsOk(bool populateOptional)
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var journeyStartDate = new DateOnly(2021, 10, 6);
        var changeReason = populateOptional ? AlertChangeStartDateReasonOption.AnotherReason : AlertChangeStartDateReasonOption.IncorrectStartDate;
        var changeReasonDetail = populateOptional ? "Some details" : null;
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate)));
        var alertId = person.Alerts.Single().AlertId;

        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertStartDateState()
            {
                Initialized = true,
                CurrentStartDate = databaseStartDate,
                StartDate = journeyStartDate,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = populateOptional,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = populateOptional ? true : false,
                EvidenceFileId = populateOptional ? evidenceFileId : null,
                EvidenceFileName = populateOptional ? evidenceFileName : null
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(journeyStartDate.ToString("d MMMM yyyy"), doc.GetSummaryListValueForKey("New start date"));
        Assert.Equal(databaseStartDate.ToString("d MMMM yyyy"), doc.GetSummaryListValueForKey("Current start date"));
        Assert.Equal(changeReason.GetDisplayName(), doc.GetSummaryListValueForKey("Reason for change"));
        Assert.Equal(populateOptional ? changeReasonDetail : "-", doc.GetSummaryListValueForKey("Reason details"));
        Assert.Equal(populateOptional ? $"{evidenceFileName} (opens in new tab)" : "-", doc.GetSummaryListValueForKey("Evidence"));
    }

    [Fact]
    public async Task Post_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/start-date", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(AlertChangeStartDateReasonOption.IncorrectStartDate)]
    [InlineData(AlertChangeStartDateReasonOption.ChangeOfStartDate)]
    [InlineData(AlertChangeStartDateReasonOption.AnotherReason)]
    public async Task Post_Confirm_UpdatesAlertCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(AlertChangeStartDateReasonOption changeReason)
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var journeyStartDate = new DateOnly(2021, 10, 6);
        var changeReasonDetail = "Some reason or other";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate)));
        var originalAlert = person.Alerts.Single();
        var alertId = originalAlert.AlertId;

        EventPublisher.Clear();

        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertStartDateState()
            {
                Initialized = true,
                CurrentStartDate = databaseStartDate,
                StartDate = journeyStartDate,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = true,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = true,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Alert changed");

        await WithDbContext(async dbContext =>
        {
            var updatedAlert = await dbContext.Alerts.FirstOrDefaultAsync(a => a.AlertId == alertId);
            Assert.Equal(journeyStartDate, updatedAlert!.StartDate);
        });

        EventPublisher.AssertEventsSaved(e =>
        {
            var expectedAlertUpdatedEvent = new AlertUpdatedEvent()
            {
                EventId = Guid.Empty,
                CreatedUtc = Clock.UtcNow,
                RaisedBy = GetCurrentUserId(),
                PersonId = person.PersonId,
                Alert = new()
                {
                    AlertId = alertId,
                    AlertTypeId = originalAlert.AlertTypeId,
                    Details = originalAlert.Details,
                    ExternalLink = originalAlert.ExternalLink,
                    StartDate = journeyStartDate,
                    EndDate = originalAlert.EndDate
                },
                OldAlert = new()
                {
                    AlertId = alertId,
                    AlertTypeId = originalAlert.AlertTypeId,
                    Details = originalAlert.Details,
                    ExternalLink = originalAlert.ExternalLink,
                    StartDate = databaseStartDate,
                    EndDate = originalAlert.EndDate
                },
                ChangeReason = changeReason.GetDisplayName(),
                ChangeReasonDetail = changeReasonDetail,
                EvidenceFile = new()
                {
                    FileId = evidenceFileId,
                    Name = evidenceFileName
                },
                Changes = AlertUpdatedEventChanges.StartDate
            };

            var actualAlertUpdatedEvent = Assert.IsType<AlertUpdatedEvent>(e);
            Assert.Equivalent(expectedAlertUpdatedEvent with { EventId = actualAlertUpdatedEvent.EventId }, actualAlertUpdatedEvent);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var databaseStartDate = new DateOnly(2021, 10, 5);
        var journeyStartDate = new DateOnly(2021, 10, 6);
        var changeReason = AlertChangeStartDateReasonOption.IncorrectStartDate;
        var changeReasonDetail = "Some reason or other";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate)));
        var alertId = person.Alerts.Single().AlertId;

        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertStartDateState()
            {
                Initialized = true,
                CurrentStartDate = databaseStartDate,
                StartDate = journeyStartDate,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = true,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = true,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var journeyStartDate = new DateOnly(2021, 10, 6);
        var changeReason = AlertChangeStartDateReasonOption.IncorrectStartDate;
        var changeReasonDetail = "Some reason or other";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate)));
        var alertId = person.Alerts.Single().AlertId;

        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertStartDateState()
            {
                Initialized = true,
                CurrentStartDate = databaseStartDate,
                StartDate = journeyStartDate,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = true,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = true,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date/check-answers/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private async Task<JourneyInstance<EditAlertStartDateState>> CreateJourneyInstance(Guid alertId, EditAlertStartDateState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditAlertStartDate,
            state ?? new EditAlertStartDateState(),
            new KeyValuePair<string, object>("alertId", alertId));
}
