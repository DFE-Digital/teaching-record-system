using TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.CloseAlert;

public class CheckAnswersTests : TestBase
{
    public CheckAnswersTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsReadWrite, UserRoles.DbsAlertsReadWrite));
    }

    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed")); // Prohibition by the Secretary of State - misconduct
        var startDate = TestData.Clock.Today.AddDays(-50);
        var details = "Some details";
        var link = TestData.GenerateUrl();
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var changeReason = CloseAlertReasonOption.EndDateSet;
        var changeReasonDetail = "Some details";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";
        var person = await TestData.CreatePerson(
            b => b.WithAlert(
                a => a.WithAlertTypeId(alertType.AlertTypeId)
                    .WithDetails(details)
                    .WithExternalLink(link)
                    .WithStartDate(startDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new CloseAlertState()
            {
                EndDate = journeyEndDate,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = true,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = true,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange        
        var person = await TestData.CreatePerson(b => b.WithAlert());
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/close", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_WithValidJourneyState_ReturnsOk(bool populateOptional)
    {
        // Arrange
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed")); // Prohibition by the Secretary of State - misconduct
        var startDate = TestData.Clock.Today.AddDays(-50);
        var details = "Some details";
        var link = populateOptional ? TestData.GenerateUrl() : null;
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var changeReason = CloseAlertReasonOption.EndDateSet;
        var changeReasonDetail = populateOptional ? "Some details" : null;
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";
        var person = await TestData.CreatePerson(
            b => b.WithAlert(
                a => a.WithAlertTypeId(alertType.AlertTypeId)
                    .WithDetails(details)
                    .WithExternalLink(link)
                    .WithStartDate(startDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new CloseAlertState()
            {
                EndDate = journeyEndDate,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = populateOptional,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = populateOptional ? true : false,
                EvidenceFileId = populateOptional ? evidenceFileId : null,
                EvidenceFileName = populateOptional ? evidenceFileName : null
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(alertType.Name, doc.GetSummaryListValueForKey("Alert type"));
        Assert.Equal(details, doc.GetSummaryListValueForKey("Details"));
        Assert.Equal(populateOptional ? $"{link} (opens in new tab)" : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("Link"));
        Assert.Equal(startDate.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("Start date"));
        Assert.Equal(journeyEndDate.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("End date"));
        Assert.Equal(changeReason.GetDisplayName(), doc.GetSummaryListValueForKey("Reason for change"));
        Assert.Equal(populateOptional ? changeReasonDetail : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("Reason details"));
        Assert.Equal(populateOptional ? $"{evidenceFileName} (opens in new tab)" : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("Evidence"));
    }

    [Fact]
    public async Task Post_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithAlert());
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/close", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(CloseAlertReasonOption.EndDateSet)]
    [InlineData(CloseAlertReasonOption.AlertPeriodHasEnded)]
    [InlineData(CloseAlertReasonOption.AlertTypeIsNoLongerValid)]
    [InlineData(CloseAlertReasonOption.AnotherReason)]
    public async Task Post_Confirm_UpdatesAlertCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(CloseAlertReasonOption changeReason)
    {
        // Arrange
        var startDate = TestData.Clock.Today.AddDays(-50);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var changeReasonDetail = "Some reason or other";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate)));
        var originalAlert = person.Alerts.Single();
        var alertId = originalAlert.AlertId;

        EventPublisher.Clear();

        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new CloseAlertState()
            {
                EndDate = journeyEndDate,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = true,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = true,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Alert closed");

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
                    EndDate = null
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
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed")); // Prohibition by the Secretary of State - misconduct
        var startDate = TestData.Clock.Today.AddDays(-50);
        var details = "Some details";
        var link = TestData.GenerateUrl();
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var changeReason = CloseAlertReasonOption.EndDateSet;
        var changeReasonDetail = "Some details";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";
        var person = await TestData.CreatePerson(
            b => b.WithAlert(
                a => a.WithAlertTypeId(alertType.AlertTypeId)
                    .WithDetails(details)
                    .WithExternalLink(link)
                    .WithStartDate(startDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new CloseAlertState()
            {
                EndDate = journeyEndDate,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = true,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = true,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var changeReason = CloseAlertReasonOption.AnotherReason;
        var changeReasonDetail = "Some reason or other";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new CloseAlertState()
            {
                EndDate = journeyEndDate,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = true,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = true,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/close/check-answers/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private async Task<JourneyInstance<CloseAlertState>> CreateJourneyInstance(Guid alertId, CloseAlertState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.CloseAlert,
            state ?? new CloseAlertState(),
            new KeyValuePair<string, object>("alertId", alertId));
}

