namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.Link;

public class CheckAnswersTests : LinkTestBase
{
    public CheckAnswersTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTraDbs));
    }

    [Theory]
    [RolesWithoutAlertWritePermissionData]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/link/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/link/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/link/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(JourneySteps.Index, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/link/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_WithValidJourneyState_ReturnsOk(bool populateOptional)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert(!populateOptional);
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/link/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(populateOptional ? $"{journeyInstance.State.Link} (opens in new tab)" : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("New link"));
        Assert.Equal(populateOptional ? UiDefaults.EmptyDisplayContent : $"{alert.ExternalLink} (opens in new tab)", doc.GetSummaryListValueForKey("Current link"));
        Assert.Equal(journeyInstance.State.ChangeReason!.Value.GetDisplayName(), doc.GetSummaryListValueForKey("Reason for change"));
        Assert.Equal(populateOptional ? journeyInstance.State.ChangeReasonDetail : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("Reason details"));
        Assert.Equal(populateOptional ? $"{journeyInstance.State.EvidenceFileName} (opens in new tab)" : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("Evidence"));
    }

    [Theory]
    [RolesWithoutAlertWritePermissionData]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: true);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/link/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: true);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(JourneySteps.Index, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_Confirm_UpdatesAlertCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(bool populateOptional)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert(!populateOptional);
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional);

        EventPublisher.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Alert changed");

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualAlertUpdatedEvent = Assert.IsType<AlertUpdatedEvent>(e);

            var expectedAlertUpdatedEvent = new AlertUpdatedEvent()
            {
                EventId = Guid.Empty,
                CreatedUtc = Clock.UtcNow,
                RaisedBy = GetCurrentUserId(),
                PersonId = person.PersonId,
                Alert = new()
                {
                    AlertId = alert.AlertId,
                    AlertTypeId = alert.AlertTypeId,
                    Details = alert.Details,
                    ExternalLink = journeyInstance.State.Link,
                    StartDate = alert.StartDate,
                    EndDate = alert.EndDate
                },
                OldAlert = new()
                {
                    AlertId = alert.AlertId,
                    AlertTypeId = alert.AlertTypeId,
                    Details = alert.Details,
                    ExternalLink = alert.ExternalLink,
                    StartDate = alert.StartDate,
                    EndDate = alert.EndDate
                },
                ChangeReason = journeyInstance.State.ChangeReason!.GetDisplayName(),
                ChangeReasonDetail = populateOptional ? journeyInstance.State.ChangeReasonDetail : null,
                EvidenceFile = populateOptional ? new()
                {
                    FileId = journeyInstance.State.EvidenceFileId!.Value,
                    Name = journeyInstance.State.EvidenceFileName!
                }
                : null,
                Changes = AlertUpdatedEventChanges.ExternalLink
            };

            Assert.Equivalent(expectedAlertUpdatedEvent, actualAlertUpdatedEvent);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: true);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/check-answers/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }
}
