using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public class CheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstance(personId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details",
            StartDate = new DateOnly(2021, 1, 1),
            UploadEvidence = false
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/check-answers?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToAlertTypePage()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/type?personId={person.PersonId}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_WithPersonIdForValidPerson_ReturnsOk(bool populateOptional)
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed")); // Prohibition by the Secretary of State - misconduct
        var details = "Some details";
        var link = TestData.GenerateUrl();
        var startDate = new DateOnly(2021, 1, 1);
        var reason = "Some reason";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = details,
            Link = populateOptional ? link : null,
            StartDate = new DateOnly(2021, 1, 1),
            Reason = populateOptional ? reason : null,
            UploadEvidence = populateOptional ? true : false,
            EvidenceFileId = populateOptional ? evidenceFileId : null,
            EvidenceFileName = populateOptional ? evidenceFileName : null
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act

        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(alertType.Name, doc.GetElementByTestId("alert-type")!.TextContent);
        Assert.Equal(details, doc.GetElementByTestId("details")!.TextContent);
        Assert.Equal(populateOptional ? $"{link} (opens in new tab)" : "-", doc.GetElementByTestId("link")!.TextContent);
        Assert.Equal(startDate.ToString("d MMMM yyyy"), doc.GetElementByTestId("start-date")!.TextContent);
        Assert.Equal(populateOptional ? reason : "-", doc.GetElementByTestId("reason")!.TextContent);
        Assert.Equal(populateOptional ? $"{evidenceFileName} (opens in new tab)" : "-", doc.GetElementByTestId("uploaded-evidence-link")!.TextContent);
    }

    [Fact]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstance(personId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details",
            StartDate = new DateOnly(2021, 1, 1),
            UploadEvidence = false
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/check-answers?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_Confirm_CreatesAlertCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var alertTypeId = (await TestData.ReferenceDataCache.GetAlertTypes()).Where(a => a.IsActive).RandomOne().AlertTypeId;
        var details = "Some details";
        var link = TestData.GenerateUrl();
        var startDate = new DateOnly(2021, 1, 1);
        var reason = "Some reason";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";

        var person = await TestData.CreatePerson();

        EventPublisher.Clear();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = alertTypeId,
            Details = details,
            Link = link,
            StartDate = startDate,
            Reason = reason,
            UploadEvidence = true,
            EvidenceFileId = evidenceFileId,
            EvidenceFileName = evidenceFileName
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Alert added");

        EventPublisher.AssertEventsSaved(e =>
        {
            var expectedAlertCreatedEvent = new AlertCreatedEvent()
            {
                EventId = Guid.Empty,
                CreatedUtc = Clock.UtcNow,
                RaisedBy = GetCurrentUserId(),
                PersonId = person.PersonId,
                AddReasonDetail = reason,
                Alert = new()
                {
                    AlertId = Guid.Empty,
                    AlertTypeId = alertTypeId,
                    Details = details,
                    ExternalLink = link,
                    StartDate = startDate,
                    EndDate = null
                },
                EvidenceFile = new()
                {
                    FileId = evidenceFileId,
                    Name = evidenceFileName
                }
            };

            var actualAlertCreatedEvent = Assert.IsType<AlertCreatedEvent>(e);
            Assert.Equivalent(expectedAlertCreatedEvent with { EventId = actualAlertCreatedEvent.EventId }, actualAlertCreatedEvent);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details",
            StartDate = new DateOnly(2022, 1, 1),
            UploadEvidence = false
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/check-answers/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private async Task<JourneyInstance<AddAlertState>> CreateJourneyInstance(Guid personId, AddAlertState? state = null) =>
        await CreateJourneyInstance(
             JourneyNames.AddAlert,
             state ?? new AddAlertState(),
             new KeyValuePair<string, object>("personId", personId));
}
