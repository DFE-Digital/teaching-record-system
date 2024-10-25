using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.EndDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.EndDate;

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

        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var changeReason = AlertChangeEndDateReasonOption.AnotherReason;

        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                CurrentEndDate = databaseEndDate,
                EndDate = journeyEndDate,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = false,
                ChangeReasonDetail = null,
                UploadEvidence = false,
                EvidenceFileId = null,
                EvidenceFileName = null
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange        
        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/end-date", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_WithValidJourneyState_ReturnsOk(bool populateOptional)
    {
        // Arrange
        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var changeReason = AlertChangeEndDateReasonOption.IncorrectEndDate;
        var changeReasonDetail = populateOptional ? "Some details" : null;
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                CurrentEndDate = databaseEndDate,
                EndDate = journeyEndDate,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = populateOptional,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = populateOptional ? true : false,
                EvidenceFileId = populateOptional ? evidenceFileId : null,
                EvidenceFileName = populateOptional ? evidenceFileName : null
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(journeyEndDate.ToString("d MMMM yyyy"), doc.GetSummaryListValueForKey("New end date"));
        Assert.Equal(databaseEndDate.ToString("d MMMM yyyy"), doc.GetSummaryListValueForKey("Current end date"));
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/end-date", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(AlertChangeEndDateReasonOption.IncorrectEndDate)]
    [InlineData(AlertChangeEndDateReasonOption.ChangeOfEndDate)]
    [InlineData(AlertChangeEndDateReasonOption.AnotherReason)]
    public async Task Post_Confirm_UpdatesAlertCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(AlertChangeEndDateReasonOption changeReason)
    {
        // Arrange
        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var changeReasonDetail = "Some reason or other";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var originalAlert = person.Alerts.Single();
        var alertId = originalAlert.AlertId;

        EventPublisher.Clear();

        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                CurrentEndDate = databaseEndDate,
                EndDate = journeyEndDate,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = true,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = true,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
            Assert.Equal(journeyEndDate, updatedAlert!.EndDate);
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
                    StartDate = originalAlert.StartDate,
                    EndDate = journeyEndDate
                },
                OldAlert = new()
                {
                    AlertId = alertId,
                    AlertTypeId = originalAlert.AlertTypeId,
                    Details = originalAlert.Details,
                    ExternalLink = originalAlert.ExternalLink,
                    StartDate = originalAlert.StartDate,
                    EndDate = databaseEndDate
                },
                ChangeReason = changeReason.GetDisplayName(),
                ChangeReasonDetail = changeReasonDetail,
                EvidenceFile = new()
                {
                    FileId = evidenceFileId,
                    Name = evidenceFileName
                },
                Changes = AlertUpdatedEventChanges.EndDate
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

        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var changeReason = AlertChangeEndDateReasonOption.AnotherReason;

        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                CurrentEndDate = databaseEndDate,
                EndDate = journeyEndDate,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = false,
                ChangeReasonDetail = null,
                UploadEvidence = false,
                EvidenceFileId = null,
                EvidenceFileName = null
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var changeReason = AlertChangeEndDateReasonOption.IncorrectEndDate;
        var changeReasonDetail = "Some reason or other";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                CurrentEndDate = databaseEndDate,
                EndDate = journeyEndDate,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = true,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = true,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/check-answers/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private async Task<JourneyInstance<EditAlertEndDateState>> CreateJourneyInstance(Guid alertId, EditAlertEndDateState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditAlertEndDate,
            state ?? new EditAlertEndDateState(),
            new KeyValuePair<string, object>("alertId", alertId));
}
