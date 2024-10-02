using TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.ReopenAlert;

public class CheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/reopen/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        var startDate = Clock.Today.AddDays(-50);
        var endDate = Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(endDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/reopen/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/reopen", response.Headers.Location?.OriginalString);
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
        var changeReason = populateOptional ? ReopenAlertReasonOption.AnotherReason : ReopenAlertReasonOption.ClosedInError;
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
            new ReopenAlertState()
            {
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = populateOptional ? true : false,
                EvidenceFileId = populateOptional ? evidenceFileId : null,
                EvidenceFileName = populateOptional ? evidenceFileName : null
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/reopen/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(alertType.Name, doc.GetElementByTestId("alert-type")!.TextContent);
        Assert.Equal(details, doc.GetElementByTestId("details")!.TextContent);
        Assert.Equal(populateOptional ? $"{link} (opens in new tab)" : "-", doc.GetElementByTestId("link")!.TextContent);
        Assert.Equal(startDate.ToString("d MMMM yyyy"), doc.GetElementByTestId("start-date")!.TextContent);
        if (changeReason == ReopenAlertReasonOption.AnotherReason)
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/reopen/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        var startDate = Clock.Today.AddDays(-50);
        var endDate = Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(endDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/reopen/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/reopen", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(ReopenAlertReasonOption.ClosedInError)]
    [InlineData(ReopenAlertReasonOption.AnotherReason)]
    public async Task Post_Confirm_UpdatesAlertCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(ReopenAlertReasonOption changeReason)
    {
        // Arrange
        var startDate = TestData.Clock.Today.AddDays(-50);
        var endDate = TestData.Clock.Today.AddDays(-5);
        var changeReasonDetail = "Some reason or other";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(endDate)));
        var originalAlert = person.Alerts.Single();
        var alertId = originalAlert.AlertId;

        EventPublisher.Clear();

        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new ReopenAlertState()
            {
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = true,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/reopen/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Alert re-opened");

        await WithDbContext(async dbContext =>
        {
            var updatedAlert = await dbContext.Alerts.FirstOrDefaultAsync(a => a.AlertId == alertId);
            Assert.Null(updatedAlert!.EndDate);
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
                    EndDate = null
                },
                OldAlert = new()
                {
                    AlertId = alertId,
                    AlertTypeId = originalAlert.AlertTypeId,
                    Details = originalAlert.Details,
                    ExternalLink = originalAlert.ExternalLink,
                    StartDate = originalAlert.StartDate,
                    EndDate = originalAlert.EndDate
                },
                ChangeReason = changeReason == ReopenAlertReasonOption.AnotherReason ? changeReasonDetail : changeReason.GetDisplayName(),
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
        var endDate = TestData.Clock.Today.AddDays(-5);
        var changeReason = ReopenAlertReasonOption.AnotherReason;
        var changeReasonDetail = "Some reason or other";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(endDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new ReopenAlertState()
            {
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = true,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/reopen/check-answers/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private async Task<JourneyInstance<ReopenAlertState>> CreateJourneyInstance(Guid alertId, ReopenAlertState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.ReopenAlert,
            state ?? new ReopenAlertState(),
            new KeyValuePair<string, object>("alertId", alertId));
}
