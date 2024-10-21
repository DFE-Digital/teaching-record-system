using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public class CheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(personId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
            StartDate = new DateOnly(2022, 1, 1),
            AddReason = AddAlertReasonOption.AnotherReason,
            HasAdditionalReasonDetail = false
        });

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
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
            StartDate = new DateOnly(2022, 1, 1)
        });

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
        var person = await TestData.CreatePerson();
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed")); // Prohibition by the Secretary of State - misconduct
        var details = "Some details";
        var link = populateOptional ? TestData.GenerateUrl() : null;
        var startDate = new DateOnly(2021, 1, 1);
        var reason = AddAlertReasonOption.AnotherReason;
        var reasonDetail = populateOptional ? "Some reason" : null;
        var evidenceFileId = populateOptional ? Guid.NewGuid() : (Guid?)null;
        var evidenceFileName = populateOptional ? "test.pdf" : null;

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = details,
            AddLink = link is not null,
            Link = link,
            StartDate = startDate,
            AddReason = reason,
            HasAdditionalReasonDetail = reasonDetail is not null,
            AddReasonDetail = reasonDetail,
            UploadEvidence = evidenceFileId is not null,
            EvidenceFileId = evidenceFileId,
            EvidenceFileName = evidenceFileName
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act

        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(alertType.Name, doc.GetSummaryListValueForKey("Alert type"));
        Assert.Equal(details, doc.GetSummaryListValueForKey("Details"));
        Assert.Equal(populateOptional ? $"{link} (opens in new tab)" : "-", doc.GetSummaryListValueForKey("Link"));
        Assert.Equal(startDate.ToString("d MMMM yyyy"), doc.GetSummaryListValueForKey("Start date"));
        Assert.Equal(reason.GetDisplayName(), doc.GetSummaryListValueForKey("Reason for adding"));
        Assert.Equal(populateOptional ? $"{evidenceFileName} (opens in new tab)" : "-", doc.GetSummaryListValueForKey("Evidence"));
    }

    [Fact]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(personId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
            StartDate = new DateOnly(2022, 1, 1),
            AddReason = AddAlertReasonOption.AnotherReason,
            HasAdditionalReasonDetail = false
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
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).Where(a => a.IsActive).RandomOne();
        var details = "Some details";
        var link = TestData.GenerateUrl();
        var startDate = new DateOnly(2021, 1, 1);
        var reason = AddAlertReasonOption.AnotherReason;
        var reasonDetail = "Reason details";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";

        var person = await TestData.CreatePerson();

        EventPublisher.Clear();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = details,
            AddLink = true,
            Link = link,
            StartDate = startDate,
            AddReason = reason,
            HasAdditionalReasonDetail = true,
            AddReasonDetail = reasonDetail,
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
                AddReason = reason.GetDisplayName(),
                AddReasonDetail = reasonDetail,
                Alert = new()
                {
                    AlertId = Guid.Empty,
                    AlertTypeId = alertType.AlertTypeId,
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
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).Where(a => a.IsActive).RandomOne();
        var details = "Some details";
        var link = TestData.GenerateUrl();
        var startDate = new DateOnly(2021, 1, 1);
        var reason = AddAlertReasonOption.AnotherReason;
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = details,
            AddLink = true,
            Link = link,
            StartDate = startDate,
            AddReason = reason,
            HasAdditionalReasonDetail = false,
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
