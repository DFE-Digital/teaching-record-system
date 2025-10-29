using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.EndDate;

public class CheckAnswersTests(HostFixture hostFixture) : EndDateTestBase(hostFixture)
{
    [Before(Test)]
    public async Task SetUser() => SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.AlertsManagerTraDbs));

    [Test]
    [RolesWithoutAlertWritePermissionData]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_WithOpenAlert_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(JourneySteps.Index, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/end-date", response.Headers.Location?.OriginalString);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Get_WithValidJourneyState_ReturnsOk(bool populateOptional)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(journeyInstance.State.EndDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueByKey("New end date"));
        Assert.Equal(alert.EndDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueByKey("Previous end date"));
        Assert.Equal(journeyInstance.State.ChangeReason!.Value.GetDisplayName(), doc.GetSummaryListValueByKey("Reason"));
        Assert.Equal(populateOptional ? journeyInstance.State.ChangeReasonDetail : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueByKey("Additional information"));
        Assert.Equal(populateOptional ? $"{journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName} (opens in new tab)" : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueByKey("Evidence"));
    }

    [Test]
    [RolesWithoutAlertWritePermissionData]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_WithOpenAlert_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(JourneySteps.Index, alert);


        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/end-date", response.Headers.Location?.OriginalString);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Post_Confirm_UpdatesAlertCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(bool populateOptional)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional);

        EventObserver.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Alert changed");

        await WithDbContextAsync(async dbContext =>
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
                    AlertId = actualAlertUpdatedEvent.Alert.AlertId,
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
                    EndDate = alert.EndDate
                },
                ChangeReason = journeyInstance.State.ChangeReason!.GetDisplayName(),
                ChangeReasonDetail = populateOptional ? journeyInstance.State.ChangeReasonDetail : null,
                EvidenceFile = populateOptional
                    ? journeyInstance.State.Evidence.UploadedEvidenceFile?.ToEventModel()
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
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert, true);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/end-date/check-answers/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Test]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert, true);

        var request = new HttpRequestMessage(httpMethod, $"/alerts/{alert.AlertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }
}
