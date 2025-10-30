using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.DeleteAlert;

public class CheckAnswersTests(HostFixture hostFixture) : DeleteAlertTestBase(hostFixture), IAsyncLifetime
{
    private const string PreviousStep = JourneySteps.Index;

    async ValueTask IAsyncLifetime.InitializeAsync() => SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.AlertsManagerTraDbs));

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: null));

        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_AlertDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_MissingDataInJourneyState_RedirectsToIndexPage(bool isOpenAlert)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert() : await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/delete", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task Get_WithValidJourneyState_ReturnsOk(bool isOpenAlert, bool populateOptional)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert(populateOptional) : await CreatePersonWithClosedAlert(populateOptional);
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert, populateOptional);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(alert.AlertType!.Name, doc.GetSummaryListValueByKey("Alert type"));
        Assert.Equal(alert.Details, doc.GetSummaryListValueByKey("Details"));
        Assert.Equal(populateOptional ? $"{alert.ExternalLink} (opens in new tab)" : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueByKey("Link"));
        Assert.Equal(alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueByKey("Start date"));
        Assert.Equal(isOpenAlert ? UiDefaults.EmptyDisplayContent : alert.EndDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueByKey("End date"));
        Assert.Equal(journeyInstance.State.DeleteReason?.GetDisplayName(), doc.GetSummaryListValueByKey("Reason"));
        Assert.Equal(populateOptional ? journeyInstance.State.DeleteReasonDetail : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueByKey("Additional information"));
        Assert.Equal(populateOptional ? $"{journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName} (opens in new tab)" : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueByKey("Evidence"));
    }

    [Theory]
    [RolesWithoutAlertWritePermissionData]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_AlertDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GPost_MissingDataInJourneyState_RedirectsToIndexPage(bool isOpenAlert)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert() : await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/delete", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_Confirm_DeletesAlertCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(bool isOpenAlert)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert() : await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert);

        EventObserver.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Alert deleted");

        await WithDbContextAsync(async dbContext =>
        {
            var alertExists = await dbContext.Alerts.AnyAsync(a => a.AlertId == alert.AlertId);
            Assert.False(alertExists);
        });

        EventObserver.AssertEventsSaved(e =>
        {
            var actualAlertDeletedEvent = Assert.IsType<AlertDeletedEvent>(e);

            var expectedAlertDeletedEvent = new AlertDeletedEvent
            {
                EventId = actualAlertDeletedEvent.EventId,
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
                    EndDate = alert.EndDate
                },
                DeletionReasonDetail = journeyInstance.State.DeleteReasonDetail,
                EvidenceFile = journeyInstance.State.Evidence.UploadedEvidenceFile?.ToEventModel()
            };


            Assert.Equivalent(expectedAlertDeletedEvent, actualAlertDeletedEvent);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_Cancel_DeletesJourneyAndRedirects(bool isOpenAlert)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert() : await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete/check-answers/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        if (isOpenAlert)
        {
            Assert.Equal($"/persons/{person.PersonId}/alerts", response.Headers.Location!.OriginalString);
        }
        else
        {
            Assert.Equal($"/alerts/{alert.AlertId}", response.Headers.Location!.OriginalString);
        }

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert);

        var request = new HttpRequestMessage(httpMethod, $"/alerts/{alert.AlertId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }
}
