
namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public class CheckAnswersTests : AddAlertTestBase
{
    private const string PreviousStep = JourneySteps.Reason;

    public CheckAnswersTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTraDbs));
    }

    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));

        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, personId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/check-answers?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToReasonPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/reason?personId={person.PersonId}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_WithPersonIdForValidPerson_ReturnsOk(bool populateOptional)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(person.PersonId, populateOptional);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(journeyInstance.State.AlertTypeName, doc.GetSummaryListValueForKey("Alert type"));
        Assert.Equal(journeyInstance.State.Details, doc.GetSummaryListValueForKey("Details"));
        Assert.Equal(populateOptional ? $"{journeyInstance.State.Link} (opens in new tab)" : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("Link"));
        Assert.Equal(journeyInstance.State.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), doc.GetSummaryListValueForKey("Start date"));
        Assert.Equal(journeyInstance.State.AddReason!.GetDisplayName(), doc.GetSummaryListValueForKey("Reason for adding"));
        Assert.Equal(populateOptional ? journeyInstance.State.AddReasonDetail : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("Reason details"));
        Assert.Equal(populateOptional ? $"{journeyInstance.State.EvidenceFileName} (opens in new tab)" : UiDefaults.EmptyDisplayContent, doc.GetSummaryListValueForKey("Evidence"));
    }

    [Theory]
    [RolesWithoutAlertWritePermissionData]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));

        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, personId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/check-answers?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_Confirm_CreatesAlertCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(bool populateOptional)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(person.PersonId, populateOptional);

        EventPublisher.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Alert added");

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualAlertCreatedEvent = Assert.IsType<AlertCreatedEvent>(e);

            var expectedAlertCreatedEvent = new AlertCreatedEvent()
            {
                EventId = actualAlertCreatedEvent.EventId,
                CreatedUtc = Clock.UtcNow,
                RaisedBy = GetCurrentUserId(),
                PersonId = person.PersonId,
                Alert = new()
                {
                    AlertId = actualAlertCreatedEvent.Alert.AlertId,
                    AlertTypeId = journeyInstance.State.AlertTypeId,
                    Details = journeyInstance.State.Details,
                    ExternalLink = journeyInstance.State.Link,
                    StartDate = journeyInstance.State.StartDate,
                    EndDate = null
                },
                AddReason = journeyInstance.State.AddReason!.GetDisplayName(),
                AddReasonDetail = populateOptional ? journeyInstance.State.AddReasonDetail : null,
                EvidenceFile = populateOptional ? new()
                {
                    FileId = journeyInstance.State.EvidenceFileId!.Value,
                    Name = journeyInstance.State.EvidenceFileName!
                }
                : null
            };

            Assert.Equivalent(expectedAlertCreatedEvent, actualAlertCreatedEvent);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(person.PersonId, populateOptional: true);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/check-answers/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Theory]
    [MemberData(nameof(HttpMethods), TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceForAllStepsCompletedAsync(person.PersonId, populateOptional: true);

        var request = new HttpRequestMessage(httpMethod, $"/alerts/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }
}
