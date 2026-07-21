using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

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
    [InlineData(true, true, true, DeleteAlertReasonOption.AnotherReason)]
    [InlineData(true, false, false, DeleteAlertReasonOption.AnotherReason)]
    [InlineData(false, true, false, DeleteAlertReasonOption.AddedInError)]
    [InlineData(false, false, true, DeleteAlertReasonOption.AddedInError)]
    public async Task Get_WithValidJourneyState_ReturnsOk(bool isOpenAlert, bool populateOptional, bool provideAdditionalInformation, DeleteAlertReasonOption deleteReason)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert(populateOptional) : await CreatePersonWithClosedAlert(populateOptional);
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(
            alert: alert,
            deleteReason: deleteReason,
            populateOptional: populateOptional,
            provideAdditionalInformation: provideAdditionalInformation);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(alert.AlertType!.Name, doc.GetSummaryListValueByKey("Alert type"));
        Assert.Equal(alert.Details, doc.GetSummaryListValueByKey("Details"));
        Assert.Equal(populateOptional ? $"{alert.ExternalLink} (opens in new tab)" : WebConstants.EmptyFallbackContent, doc.GetSummaryListValueByKey("Link"));
        Assert.Equal(alert.StartDate!.Value.ToString(WebConstants.DateDisplayFormat), doc.GetSummaryListValueByKey("Start date"));
        Assert.Equal(isOpenAlert ? WebConstants.EmptyFallbackContent : alert.EndDate!.Value.ToString(WebConstants.DateDisplayFormat), doc.GetSummaryListValueByKey("End date"));
        Assert.Equal(journeyInstance.State.DeleteReason?.GetDisplayName(), doc.GetSummaryListValueByKey("Reason"));
        //Assert.Equal(populateOptional ? journeyInstance.State.DeleteReasonDetail : WebConstants.EmptyFallbackContent, doc.GetSummaryListValueByKey("Additional information"));
        Assert.Equal(populateOptional ? $"{journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName} (opens in new tab)" : WebConstants.EmptyFallbackContent, doc.GetSummaryListValueByKey("Evidence"));
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
    public async Task Post_Confirm_DeletesAlertCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(bool isOpenAlert)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert() : await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(alert);

        EventObserver.Clear();

        var state = journeyInstance.State;

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, "Alert deleted");

        await WithDbContextAsync(async dbContext =>
        {
            var alertExists = await dbContext.Alerts.AnyAsync(a => a.AlertId == alert.AlertId);
            Assert.False(alertExists);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.AlertDeleting, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<AlertDeletedEvent>();

            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(p.ProcessContext.Process.ChangeReason);
            Assert.Equal(state.DeleteReason!.GetDisplayName(), changeReason.Reason);
            Assert.Equal(state.DeleteReasonDetail, changeReason.Details);
            Assert.Equal(state.Evidence.UploadedEvidenceFile?.ToEventModel(), changeReason.EvidenceFile);
        });

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_Cancel_DeletesJourneyAndRedirects(bool isOpenAlert)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert() : await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };

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

        Assert.Null(GetJourneyInstanceState(journeyInstance));
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
