namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.CloseAlert;

public class CheckAnswersTests(HostFixture hostFixture) : CloseAlertTestBase(hostFixture)
{
    [Before(Test)]
    public async Task SetUser() => SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.AlertsManagerTraDbs));

    [Test]
    [RolesWithoutAlertWritePermissionData]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(JourneySteps.Index, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/close", response.Headers.Location?.OriginalString);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Get_WithValidJourneyState_ReturnsOk(bool populateOptional)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert(populateOptional);
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(alert.AlertType!.Name, doc.GetSummaryListValueForKey("Alert type"));
        Assert.Equal(alert.Details, doc.GetSummaryListValueForKey("Details"));
        Assert.Equal(populateOptional ? $"{alert.ExternalLink} (opens in new tab)" : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("Link"));
        Assert.Equal(alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("Start date"));
        Assert.Equal(journeyInstance.State.EndDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("End date"));
        Assert.Equal(journeyInstance.State.ChangeReason!.Value.GetDisplayName(), doc.GetSummaryListValueForKey("Reason for change"));
        Assert.Equal(populateOptional ? journeyInstance.State.ChangeReasonDetail : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("Reason details"));
        Assert.Equal(populateOptional ? $"{journeyInstance.State.EvidenceFileName} (opens in new tab)" : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("Evidence"));
    }

    [Test]
    [RolesWithoutAlertWritePermissionData]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/close", response.Headers.Location?.OriginalString);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]

    public async Task Post_Confirm_UpdatesAlertCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(bool populateOptional)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert(!populateOptional);
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional);

        EventObserver.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Alert closed");

        await WithDbContext(async dbContext =>
        {
            var updatedAlert = await dbContext.Alerts.FirstOrDefaultAsync(a => a.AlertId == alert.AlertId);
            Assert.Equal(journeyInstance.State.EndDate, updatedAlert!.EndDate);
        });

        EventObserver.AssertEventsSaved(e =>
        {
            var actualAlertUpdatedEvent = Assert.IsType<AlertUpdatedEvent>(e);

            var expectedAlertUpdatedEvent = new AlertUpdatedEvent
            {
                EventId = actualAlertUpdatedEvent.EventId,
                CreatedUtc = Clock.UtcNow,
                RaisedBy = GetCurrentUserId(),
                PersonId = person.PersonId,
                Alert = new()
                {
                    AlertId = alert.AlertId,
                    AlertTypeId = alert.AlertTypeId,
                    Details = alert.Details,
                    ExternalLink = alert.ExternalLink,
                    StartDate = alert.StartDate,
                    EndDate = journeyInstance.State.EndDate
                },
                OldAlert = new()
                {
                    AlertId = alert.AlertId,
                    AlertTypeId = alert.AlertTypeId,
                    Details = alert.Details,
                    ExternalLink = alert.ExternalLink,
                    StartDate = alert.StartDate,
                    EndDate = null
                },
                ChangeReason = journeyInstance.State.ChangeReason!.GetDisplayName(),
                ChangeReasonDetail = populateOptional ? journeyInstance.State.ChangeReasonDetail : null,
                EvidenceFile = populateOptional ? new()
                {
                    FileId = journeyInstance.State.EvidenceFileId!.Value,
                    Name = journeyInstance.State.EvidenceFileName!
                }
                : null,
                Changes = AlertUpdatedEventChanges.EndDate
            };

            Assert.Equivalent(expectedAlertUpdatedEvent, actualAlertUpdatedEvent);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Test]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: true);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/close/check-answers/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Test]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional: true);

        var request = new HttpRequestMessage(httpMethod, $"/alerts/{alert.AlertId}/close/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }
}

