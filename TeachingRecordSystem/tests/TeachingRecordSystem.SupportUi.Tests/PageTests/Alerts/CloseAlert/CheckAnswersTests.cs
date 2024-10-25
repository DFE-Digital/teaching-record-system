using TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.CloseAlert;

public class CheckAnswersTests : TestBase
{
    public CheckAnswersTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsReadWrite, UserRoles.DbsAlertsReadWrite));
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
        var changeReason = populateOptional ? CloseAlertReasonOption.AnotherReason : CloseAlertReasonOption.EndDateSet;
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
        Assert.Equal(alertType.Name, doc.GetElementByTestId("alert-type")!.TextContent);
        Assert.Equal(details, doc.GetElementByTestId("details")!.TextContent);
        Assert.Equal(populateOptional ? $"{link} (opens in new tab)" : "-", doc.GetElementByTestId("link")!.TextContent);
        Assert.Equal(startDate.ToString("d MMMM yyyy"), doc.GetElementByTestId("start-date")!.TextContent);
        Assert.Equal(journeyEndDate.ToString("d MMMM yyyy"), doc.GetElementByTestId("end-date")!.TextContent);
        if (changeReason == CloseAlertReasonOption.AnotherReason)
        {
            Assert.Equal(changeReasonDetail, doc.GetElementByTestId("change-reason")!.TextContent);
        }
        else
        {
            Assert.Equal(changeReason.GetDisplayName(), doc.GetElementByTestId("change-reason")!.TextContent);
        }
        Assert.Equal(populateOptional ? $"{evidenceFileName} (opens in new tab)" : "-", doc.GetElementByTestId("uploaded-evidence-link")!.TextContent);
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
                ChangeReason = null,
                ChangeReasonDetail = changeReason == CloseAlertReasonOption.AnotherReason ? changeReasonDetail : changeReason.GetDisplayName(),
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

