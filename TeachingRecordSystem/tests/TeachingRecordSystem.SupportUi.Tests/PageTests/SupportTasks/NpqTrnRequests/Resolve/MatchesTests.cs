using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests.Resolve;

public class MatchesTests(HostFixture hostFixture) : ResolveNpqTrnRequestTestBase(hostFixture)
{
    [Fact]
    public async Task Get_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var taskReference = SupportTask.GenerateSupportTaskReference();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/npq-trn-requests/{taskReference}/matches");

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
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches");

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
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var firstMatch = await WithDbContext(
            dbContext => dbContext.Persons.SingleAsync(
                p => p.PersonId == supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId));

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithTrn().WithAlert());
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithMatchedRecords(matchedPerson.PersonId));

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        var tags = firstMatchDetails.GetSummaryListValueElementForKey("Status")?.GetElementsByClassName("govuk-tag").Select(e => e.TrimmedText()) ?? [];
        Assert.Contains("Alerts", tags);
    }

    [Fact]
    public async Task Get_MatchedRecordHasQts_ShowsQtsTag()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithQts());
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithMatchedRecords(matchedPerson.PersonId));

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        var tags = firstMatchDetails.GetSummaryListValueElementForKey("Status")?.GetElementsByClassName("govuk-tag").Select(e => e.TrimmedText()) ?? [];
        Assert.Contains("QTS", tags);
    }

    [Fact]
    public async Task Get_MatchedRecordHasEyts_ShowsEytsTag()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithEyts(Clock.Today.AddDays(-1)));
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithMatchedRecords(matchedPerson.PersonId));

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        var tags = firstMatchDetails.GetSummaryListValueElementForKey("Status")?.GetElementsByClassName("govuk-tag").Select(e => e.TrimmedText()) ?? [];
        Assert.Contains("EYTS", tags);
    }

    [Fact]
    public async Task Get_PersonIdInState_SelectsChosenRecord()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var firstMatchId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId;

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState() { PersonId = firstMatchId });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState() { PersonId = ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        Assert.True(
            doc.GetElementsByName("PersonId")
                .Single(e => e.GetAttribute("value") == ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel.ToString())
                .IsChecked());
    }

    public static TheoryData<PersonMatchedAttribute[]> HighlightedDifferencesData { get; } = new()
    {
        // We could go nuts creating loads of combinations here, but checking every attribute once seems sufficient
        new[] { PersonMatchedAttribute.FirstName },
        new[] { PersonMatchedAttribute.MiddleName },
        new[] { PersonMatchedAttribute.LastName },
        new[] { PersonMatchedAttribute.DateOfBirth },
        new[] { PersonMatchedAttribute.EmailAddress },
        new[] { PersonMatchedAttribute.NationalInsuranceNumber }
    };

    [Theory]
    [MemberData(nameof(HighlightedDifferencesData))]
    public async Task Get_HighlightsDifferencesBetweenMatchAndTrnRequest(IReadOnlyCollection<PersonMatchedAttribute> matchedAttributes)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithTrn().WithNationalInsuranceNumber());

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t
                .WithMatchedRecords(matchedPerson.PersonId)
                .WithFirstName(
                    matchedAttributes.Contains(PersonMatchedAttribute.FirstName)
                        ? matchedPerson.FirstName
                        : TestData.GenerateChangedFirstName(matchedPerson.FirstName))
                .WithMiddleName(
                    matchedAttributes.Contains(PersonMatchedAttribute.MiddleName)
                        ? matchedPerson.MiddleName
                        : TestData.GenerateChangedMiddleName(matchedPerson.MiddleName))
                .WithLastName(
                    matchedAttributes.Contains(PersonMatchedAttribute.LastName)
                        ? matchedPerson.LastName
                        : TestData.GenerateChangedLastName(matchedPerson.LastName))
                .WithDateOfBirth(
                    matchedAttributes.Contains(PersonMatchedAttribute.DateOfBirth)
                        ? matchedPerson.DateOfBirth
                        : TestData.GenerateChangedDateOfBirth(matchedPerson.DateOfBirth))
                .WithEmailAddress(
                    matchedAttributes.Contains(PersonMatchedAttribute.EmailAddress)
                        ? matchedPerson.Email
                        : TestData.GenerateUniqueEmail())
                .WithNationalInsuranceNumber(
                    matchedAttributes.Contains(PersonMatchedAttribute.NationalInsuranceNumber)
                        ? matchedPerson.NationalInsuranceNumber
                        : TestData.GenerateChangedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber!)));

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        AssertMatchRowHasExpectedHighlight("First name", !matchedAttributes.Contains(PersonMatchedAttribute.FirstName));
        AssertMatchRowHasExpectedHighlight("Middle name", !matchedAttributes.Contains(PersonMatchedAttribute.MiddleName));
        AssertMatchRowHasExpectedHighlight("Last name", !matchedAttributes.Contains(PersonMatchedAttribute.LastName));
        AssertMatchRowHasExpectedHighlight("Date of birth", !matchedAttributes.Contains(PersonMatchedAttribute.DateOfBirth));
        AssertMatchRowHasExpectedHighlight("Email", !matchedAttributes.Contains(PersonMatchedAttribute.EmailAddress));
        AssertMatchRowHasExpectedHighlight("National Insurance number", !matchedAttributes.Contains(PersonMatchedAttribute.NationalInsuranceNumber));

        void AssertMatchRowHasExpectedHighlight(string summaryListKey, bool expectHighlight)
        {
            var valueElement = firstMatchDetails.GetSummaryListValueElementForKey(summaryListKey);
            Assert.NotNull(valueElement);
            var highlightElement = valueElement.GetElementsByClassName("hods-highlight").SingleOrDefault();

            if (expectHighlight)
            {
                Assert.NotNull(highlightElement);
            }
            else
            {
                Assert.Null(highlightElement);
            }
        }
    }

    [Fact]
    public async Task Post_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithStatus(SupportTaskStatus.Closed));

        var personId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId;

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var unmatchedPerson = await TestData.CreatePersonAsync();
        var personId = unmatchedPerson.PersonId;

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var personId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId;

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder() { { "PersonId", personId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(personId, journeyInstance.State.PersonId);
    }

    [Fact]
    public async Task Post_CreateNewRecordChosen_UpdatesStateAndRedirects()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var personId = ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel;

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder() { { "PersonId", personId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(personId, journeyInstance.State.PersonId);
    }

    [Fact]
    public async Task Post_PersonIdChangedFromState_ClearsPersonAttributes()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var selectedPersonId = ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel;

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedRecords.First().PersonId,
                FirstNameSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                MiddleNameSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                LastNameSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                DateOfBirthSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                EmailAddressSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                NationalInsuranceNumberSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder() { { "PersonId", selectedPersonId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance.State.FirstNameSource);
        Assert.Null(journeyInstance.State.MiddleNameSource);
        Assert.Null(journeyInstance.State.LastNameSource);
        Assert.Null(journeyInstance.State.DateOfBirthSource);
        Assert.Null(journeyInstance.State.EmailAddressSource);
        Assert.Null(journeyInstance.State.NationalInsuranceNumberSource);
        Assert.False(journeyInstance.State.PersonAttributeSourcesSet);
    }

    [Fact]
    public async Task Post_PersonIdNotChangedFromState_DoesNotClearPersonAttributes()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var selectedPersonId = ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel;

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                PersonId = selectedPersonId,
                FirstNameSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                MiddleNameSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                LastNameSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                DateOfBirthSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                EmailAddressSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                NationalInsuranceNumberSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder() { { "PersonId", selectedPersonId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.NotNull(journeyInstance.State.FirstNameSource);
        Assert.NotNull(journeyInstance.State.MiddleNameSource);
        Assert.NotNull(journeyInstance.State.LastNameSource);
        Assert.NotNull(journeyInstance.State.DateOfBirthSource);
        Assert.NotNull(journeyInstance.State.EmailAddressSource);
        Assert.NotNull(journeyInstance.State.NationalInsuranceNumberSource);
        Assert.True(journeyInstance.State.PersonAttributeSourcesSet);
    }

    [Fact]
    public async Task Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var selectedPersonId = ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel;

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                PersonId = selectedPersonId,
                FirstNameSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                MiddleNameSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                LastNameSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                DateOfBirthSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                EmailAddressSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                NationalInsuranceNumberSource = ResolveNpqTrnRequestState.PersonAttributeSource.ExistingRecord,
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, cancelButton!.FormAction);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        var location = redirectResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/support-tasks", location);
        Assert.Null(await ReloadJourneyInstance(journeyInstance));
    }

    private Task<JourneyInstance<ResolveNpqTrnRequestState>> CreateJourneyInstance(
            string supportTaskReference,
            ResolveNpqTrnRequestState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.ResolveNpqTrnRequest,
            state ?? new(),
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));
}
