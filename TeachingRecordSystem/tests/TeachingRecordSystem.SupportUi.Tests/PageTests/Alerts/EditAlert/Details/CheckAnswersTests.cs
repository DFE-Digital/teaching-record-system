using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.Details;

public class CheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var originalAlert = person.Alerts.Single();
        var alertId = originalAlert.AlertId;
        var changeReason = AlertChangeDetailsReasonOption.IncorrectDetails;
        var hasAdditionalReasonDetail = true;
        var changeReasonDetail = "My Reason";
        var uploadEvidence = true;
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileSizeDescription = "1 MB";

        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                Details = journeyDetails,
                CurrentDetails = databaseDetails,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = hasAdditionalReasonDetail,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = uploadEvidence,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName,
                EvidenceFileSizeDescription = evidenceFileSizeDescription
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_WithValidJourneyState_ReturnsOk(bool populateOptional)
    {
        var databaseDetails = populateOptional ? TestData.GenerateLoremIpsum() : null;
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var reason = AlertChangeDetailsReasonOption.IncorrectDetails;
        var hasAdditionalReasonDetail = populateOptional;
        var reasonDetail = "My Reason";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileSizeDescription = "1 MB";

        var journeyInstance = await CreateJourneyInstance(alertId, state: new()
        {
            Initialized = true,
            Details = journeyDetails,
            CurrentDetails = databaseDetails,
            ChangeReason = reason,
            HasAdditionalReasonDetail = hasAdditionalReasonDetail,
            ChangeReasonDetail = populateOptional ? reasonDetail : null,
            UploadEvidence = populateOptional ? true : false,
            EvidenceFileId = populateOptional ? evidenceFileId : null,
            EvidenceFileName = populateOptional ? evidenceFileName : null,
            EvidenceFileSizeDescription = populateOptional ? evidenceFileSizeDescription : null
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(journeyDetails, doc.GetSummaryListValueForKey("New details"));
        Assert.Equal(populateOptional ? databaseDetails : "-", doc.GetSummaryListValueForKey("Current details"));
        Assert.Equal(reason.GetDisplayName(), doc.GetSummaryListValueForKey("Reason for change"));
        Assert.Equal(populateOptional ? reasonDetail : "-", doc.GetSummaryListValueForKey("Reason details"));
        Assert.Equal(populateOptional ? $"{evidenceFileName} (opens in new tab)" : "-", doc.GetSummaryListValueForKey("Evidence"));
    }

    [Fact]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var originalAlert = person.Alerts.Single();
        var alertId = originalAlert.AlertId;
        var changeReason = AlertChangeDetailsReasonOption.IncorrectDetails;
        var hasAdditionalReasonDetail = true;
        var changeReasonDetail = "My Reason";
        var uploadEvidence = true;
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileSizeDescription = "1 MB";

        EventPublisher.Clear();

        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                Details = journeyDetails,
                CurrentDetails = databaseDetails,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = hasAdditionalReasonDetail,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = uploadEvidence,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName,
                EvidenceFileSizeDescription = evidenceFileSizeDescription
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_Confirm_UpdatesAlertCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(bool populateOptional)
    {
        var databaseDetails = populateOptional ? TestData.GenerateLoremIpsum() : null;
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var originalAlert = person.Alerts.Single();
        var alertId = originalAlert.AlertId;
        var changeReason = AlertChangeDetailsReasonOption.IncorrectDetails;
        var hasAdditionalReasonDetail = populateOptional;
        var changeReasonDetail = populateOptional ? "My Reason" : null;
        var uploadEvidence = populateOptional ? true : false;
        var evidenceFileId = populateOptional ? Guid.NewGuid() : (Guid?)null;
        var evidenceFileName = populateOptional ? "evidence.jpg" : null;
        var evidenceFileSizeDescription = populateOptional ? "1 MB" : null;

        EventPublisher.Clear();

        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                Details = journeyDetails,
                CurrentDetails = databaseDetails,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = hasAdditionalReasonDetail,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = uploadEvidence,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName,
                EvidenceFileSizeDescription = evidenceFileSizeDescription
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Alert changed");

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
                    Details = journeyDetails,
                    ExternalLink = originalAlert.ExternalLink,
                    StartDate = originalAlert.StartDate,
                    EndDate = originalAlert.EndDate
                },
                OldAlert = new()
                {
                    AlertId = alertId,
                    AlertTypeId = originalAlert.AlertTypeId,
                    Details = databaseDetails,
                    ExternalLink = originalAlert.ExternalLink,
                    StartDate = originalAlert.StartDate,
                    EndDate = originalAlert.EndDate
                },
                ChangeReason = changeReason.GetDisplayName(),
                ChangeReasonDetail = changeReasonDetail,
                EvidenceFile = populateOptional
                    ? new()
                    {
                        FileId = evidenceFileId!.Value,
                        Name = evidenceFileName!
                    }
                    : null,
                Changes = AlertUpdatedEventChanges.Details
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
        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var originalAlert = person.Alerts.Single();
        var alertId = originalAlert.AlertId;
        var changeReason = AlertChangeDetailsReasonOption.IncorrectDetails;
        var hasAdditionalReasonDetail = true;
        var changeReasonDetail = "My Reason";
        var uploadEvidence = true;
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileSizeDescription = "1 MB";

        EventPublisher.Clear();

        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                Details = journeyDetails,
                CurrentDetails = databaseDetails,
                ChangeReason = changeReason,
                HasAdditionalReasonDetail = hasAdditionalReasonDetail,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = uploadEvidence,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName,
                EvidenceFileSizeDescription = evidenceFileSizeDescription
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/check-answers/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private Task<JourneyInstance<EditAlertDetailsState>> CreateJourneyInstance(Guid alertId, string currentDetails) =>
        CreateJourneyInstance(
            alertId,
            new EditAlertDetailsState()
            {
                Initialized = true,
                CurrentDetails = currentDetails,
                Details = currentDetails
            });

    private async Task<JourneyInstance<EditAlertDetailsState>> CreateJourneyInstance(Guid alertId, EditAlertDetailsState state) =>
        await CreateJourneyInstance(
            JourneyNames.EditAlertDetails,
            state,
            new KeyValuePair<string, object>("alertId", alertId));
}
