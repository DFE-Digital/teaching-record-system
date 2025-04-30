using AngleSharp.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ApiTrnRequests;

public class MatchesTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var taskReference = SupportTask.GenerateSupportTaskReference();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/api-trn-requests/{taskReference}/matches");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ShowsRequestDetails()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var requestDetails = doc.GetElementByTestId("request");
        Assert.NotNull(requestDetails);
        Assert.Equal(requestDetails.GetSummaryListValueForKey("First name"), supportTask.TrnRequestMetadata!.FirstName);
        Assert.Equal(requestDetails.GetSummaryListValueForKey("Middle name"), supportTask.TrnRequestMetadata!.MiddleName);
        Assert.Equal(requestDetails.GetSummaryListValueForKey("Last name"), supportTask.TrnRequestMetadata!.LastName);
        Assert.Equal(requestDetails.GetSummaryListValueForKey("Date of birth"), supportTask.TrnRequestMetadata!.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat));
        Assert.Equal(requestDetails.GetSummaryListValueForKey("Email"), supportTask.TrnRequestMetadata!.EmailAddress);
        Assert.Equal(requestDetails.GetSummaryListValueForKey("National Insurance number"), supportTask.TrnRequestMetadata!.NationalInsuranceNumber);
        // TODO Gender
    }

    [Fact]
    public async Task Get_ValidRequest_ShowsDetailsOfMatchedRecords()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var firstMatch = await WithDbContext(
            dbContext => dbContext.Persons.SingleAsync(
                p => p.PersonId == supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId));

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        Assert.Equal(firstMatchDetails.GetSummaryListValueForKey("First name"), firstMatch.FirstName);
        Assert.Equal(firstMatchDetails.GetSummaryListValueForKey("Middle name"), firstMatch.MiddleName);
        Assert.Equal(firstMatchDetails.GetSummaryListValueForKey("Last name"), firstMatch.LastName);
        Assert.Equal(firstMatchDetails.GetSummaryListValueForKey("Date of birth"), firstMatch.DateOfBirth?.ToString(UiDefaults.DateOnlyDisplayFormat));
        Assert.Equal(firstMatchDetails.GetSummaryListValueForKey("Email"), firstMatch.EmailAddress);
        Assert.Equal(firstMatchDetails.GetSummaryListValueForKey("National Insurance number"), firstMatch.NationalInsuranceNumber);
        // TODO Gender
    }

    [Fact]
    public async Task Get_MatchedRecordHasActiveAlerts_ShowsAlertsTag()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithAlert());
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithMatchedRecords(matchedPerson.PersonId));

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        var tags = firstMatchDetails.GetSummaryListValueElementForKey("Status")?.GetElementsByClassName("govuk-tag").Select(e => e.TextContent) ?? [];
        Assert.Contains("Alerts", tags);
    }

    [Fact]
    public async Task Get_MatchedRecordHasQts_ShowsQtsTag()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithQts());
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithMatchedRecords(matchedPerson.PersonId));

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        var tags = firstMatchDetails.GetSummaryListValueElementForKey("Status")?.GetElementsByClassName("govuk-tag").Select(e => e.TextContent) ?? [];
        Assert.Contains("QTS", tags);
    }

    [Fact]
    public async Task Get_MatchedRecordHasEyts_ShowsQtsTag()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithEyts(Clock.Today.AddDays(-1), "222"));
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithMatchedRecords(matchedPerson.PersonId));

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        var tags = firstMatchDetails.GetSummaryListValueElementForKey("Status")?.GetElementsByClassName("govuk-tag").Select(e => e.TextContent) ?? [];
        Assert.Contains("EYTS", tags);
    }

    [Fact]
    public async Task Get_PersonIdInState_SelectsChosenRecord()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var firstMatchId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId;

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState() { PersonId = firstMatchId });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        Assert.True(
            doc.GetElementsByName("PersonId")
                .Single(e => e.GetAttribute("value") == firstMatchId.ToString())
                .IsChecked());
    }

    [Fact]
    public async Task Get_CreateNewRecordInState_SelectsCreateNewRecordOption()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState() { PersonId = ResolveApiTrnRequestState.CreateNewRecordPersonIdSentinel });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        Assert.True(
            doc.GetElementsByName("PersonId")
                .Single(e => e.GetAttribute("value") == ResolveApiTrnRequestState.CreateNewRecordPersonIdSentinel.ToString())
                .IsChecked());
    }

    [Fact]
    public async Task Post_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithStatus(SupportTaskStatus.Closed));

        var personId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId;

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder() { { "PersonId", personId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_SubmittedPersonIdIsNotValid_ReturnsBadRequest()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var unmatchedPerson = await TestData.CreatePersonAsync();
        var personId = unmatchedPerson.PersonId;

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder() { { "PersonId", personId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_NoChosenPersonId_ReturnsError()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "PersonId", "Select a record");
    }

    [Fact]
    public async Task Post_ValidPersonIdChosen_UpdatesStateAndRedirects()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var personId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId;

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder() { { "PersonId", personId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(personId, journeyInstance.State.PersonId);
    }

    [Fact]
    public async Task Post_CreateNewRecordChosen_UpdatesStateAndRedirects()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var personId = ResolveApiTrnRequestState.CreateNewRecordPersonIdSentinel;

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder() { { "PersonId", personId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(personId, journeyInstance.State.PersonId);
    }

    private Task<JourneyInstance<ResolveApiTrnRequestState>> CreateJourneyInstance(
            string supportTaskReference,
            ResolveApiTrnRequestState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.ResolveApiTrnRequest,
            state ?? new(),
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));
}

