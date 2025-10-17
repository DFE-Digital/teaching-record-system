using AngleSharp.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ApiTrnRequests.Resolve;

public class MatchesTests(HostFixture hostFixture) : ResolveApiTrnRequestTestBase(hostFixture)
{
    [Test]
    public async Task Get_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var taskReference = SupportTask.GenerateSupportTaskReference();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/api-trn-requests/{taskReference}/resolve/matches");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_ValidRequest_ShowsRequestDetails()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, s => s
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithMiddleName(TestData.GenerateMiddleName())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber())
            .WithGender(TestData.GenerateGender()));

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var requestDetails = doc.GetElementByTestId("request");
        Assert.NotNull(requestDetails);
        Assert.Equal(supportTask.TrnRequestMetadata!.FirstName, requestDetails.GetSummaryListValueForKey("First name"));
        Assert.Equal(supportTask.TrnRequestMetadata!.MiddleName, requestDetails.GetSummaryListValueForKey("Middle name"));
        Assert.Equal(supportTask.TrnRequestMetadata!.LastName, requestDetails.GetSummaryListValueForKey("Last name"));
        Assert.Equal(supportTask.TrnRequestMetadata!.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), requestDetails.GetSummaryListValueForKey("Date of birth"));
        Assert.Equal(supportTask.TrnRequestMetadata!.EmailAddress, requestDetails.GetSummaryListValueForKey("Email address"));
        Assert.Equal(supportTask.TrnRequestMetadata!.NationalInsuranceNumber, requestDetails.GetSummaryListValueForKey("NI number"));
        Assert.Equal(supportTask.TrnRequestMetadata!.Gender?.GetDisplayName(), requestDetails.GetSummaryListValueForKey("Gender"));
    }

    [Test]
    public async Task Get_ValidRequest_ShowsDetailsOfMatchedRecords()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, s => s
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithMiddleName(TestData.GenerateMiddleName())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber())
            .WithGender(TestData.GenerateGender()));

        var firstMatch = await WithDbContext(
            dbContext => dbContext.Persons.SingleAsync(
                p => p.PersonId == supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId));

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        Assert.Equal(firstMatch.FirstName, firstMatchDetails.GetSummaryListValueForKey("First name"));
        Assert.Equal(firstMatch.MiddleName, firstMatchDetails.GetSummaryListValueForKey("Middle name"));
        Assert.Equal(firstMatch.LastName, firstMatchDetails.GetSummaryListValueForKey("Last name"));
        Assert.Equal(firstMatch.DateOfBirth?.ToString(UiDefaults.DateOnlyDisplayFormat), firstMatchDetails.GetSummaryListValueForKey("Date of birth"));
        Assert.Equal(firstMatch.EmailAddress, firstMatchDetails.GetSummaryListValueForKey("Email address"));
        Assert.Equal(firstMatch.NationalInsuranceNumber, firstMatchDetails.GetSummaryListValueForKey("NI number"));
        Assert.Equal(firstMatch.Gender?.GetDisplayName(), firstMatchDetails.GetSummaryListValueForKey("Gender"));
    }

    [Test]
    public async Task Get_MatchedRecordHasActiveAlerts_ShowsAlertsTag()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithAlert());
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, matchedPerson.Person);

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        var tags = firstMatchDetails.GetSummaryListValueElementForKey("Status")?.GetElementsByClassName("govuk-tag").Select(e => e.TrimmedText()) ?? [];
        Assert.Contains("Alerts", tags);
    }

    [Test]
    public async Task Get_MatchedRecordHasQts_ShowsQtsTag()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithQts());
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, matchedPerson.Person);

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        var tags = firstMatchDetails.GetSummaryListValueElementForKey("Status")?.GetElementsByClassName("govuk-tag").Select(e => e.TrimmedText()) ?? [];
        Assert.Contains("QTS", tags);
    }

    [Test]
    public async Task Get_MatchedRecordHasEyts_ShowsEytsTag()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithEyts(Clock.Today.AddDays(-1)));
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, matchedPerson.Person);

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        var tags = firstMatchDetails.GetSummaryListValueElementForKey("Status")?.GetElementsByClassName("govuk-tag").Select(e => e.TrimmedText()) ?? [];
        Assert.Contains("EYTS", tags);
    }

    [Test]
    public async Task Get_PersonIdInState_SelectsChosenRecord()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var firstMatchId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId;

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = firstMatchId
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        Assert.True(
            doc.GetElementsByName("PersonId")
                .Single(e => e.GetAttribute("value") == firstMatchId.ToString())
                .IsChecked());
    }

    [Test]
    public async Task Get_CreateNewRecordInState_SelectsCreateNewRecordOption()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = ResolveApiTrnRequestState.CreateNewRecordPersonIdSentinel
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        Assert.True(
            doc.GetElementsByName("PersonId")
                .Single(e => e.GetAttribute("value") == ResolveApiTrnRequestState.CreateNewRecordPersonIdSentinel.ToString())
                .IsChecked());
    }

    [Test]
    public async Task Get_MatchedRecords_NullableFieldsEmptyInRecordAndEmptyInRequest_ShowsNotProvidedNotHighlighted()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var matchedPerson = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(null)
            .WithNationalInsuranceNumber(false)
            .WithGender(false)
            .WithMiddleName(""));

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, configure =>
        {
            configure.WithMiddleName(null);
            configure.WithEmailAddress(null);
            configure.WithNationalInsuranceNumber(null);
            configure.WithGender(null);
            configure.WithMatchedPersons(matchedPerson.PersonId);
        });

        var journeyInstance = await CreateJourneyInstance(supportTask, useFactory: false);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        Assert.Equal("Not provided", firstMatchDetails.GetSummaryListValueForKey("Email address"));
        Assert.Equal("Not provided", firstMatchDetails.GetSummaryListValueForKey("Middle name"));
        Assert.Equal("Not provided", firstMatchDetails.GetSummaryListValueForKey("NI number"));
        Assert.Equal("Not provided", firstMatchDetails.GetSummaryListValueForKey("Gender"));

        AssertMatchRowHasExpectedHighlight("Middle name", false);
        AssertMatchRowHasExpectedHighlight("NI number", false);
        AssertMatchRowHasExpectedHighlight("Gender", false);
        AssertMatchRowHasExpectedHighlight("Email address", false);

        void AssertMatchRowHasExpectedHighlight(string summaryListKey, bool expectHighlight)
        {
            var valueElement = firstMatchDetails.GetSummaryListValueElementForKey(summaryListKey);
            Assert.NotNull(valueElement);
            var highlightElement = valueElement.GetElementsByClassName("hods-highlight").SingleOrDefault();

            if (expectHighlight)
            {
                Assert.False(highlightElement == null, $"{summaryListKey} should be highlighted");
            }
            else
            {
                Assert.True(highlightElement == null, $"{summaryListKey} should not be highlighted");
            }
        }
    }

    [Test]
    public async Task Get_ShowsRefreshedMatchedPersons()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var initialMatchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId, t => t
                .WithFirstName(initialMatchedPerson.FirstName)
                .WithLastName(initialMatchedPerson.LastName)
                .WithDateOfBirth(initialMatchedPerson.DateOfBirth)
                .WithNationalInsuranceNumber(initialMatchedPerson.NationalInsuranceNumber)
                .WithMatchedPersons(initialMatchedPerson.PersonId));

        var subsequentMatchedPerson = await TestData.CreatePersonAsync(p => p
            .WithFirstName(initialMatchedPerson.FirstName)
            .WithLastName(initialMatchedPerson.LastName)
            .WithDateOfBirth(initialMatchedPerson.DateOfBirth)
            .WithNationalInsuranceNumber(initialMatchedPerson.NationalInsuranceNumber!));

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var shownMatchedPersonIds = doc.GetAllElementsByTestId("match").Select(e => e.GetAttribute("data-personid")!).Select(Guid.Parse).ToList();
        Assert.Contains(initialMatchedPerson.PersonId, shownMatchedPersonIds);
        Assert.Contains(subsequentMatchedPerson.PersonId, shownMatchedPersonIds);
    }

    public static PersonMatchedAttribute[][] GetHighlightedDifferencesData() =>
    [
        [PersonMatchedAttribute.FirstName],
        [PersonMatchedAttribute.MiddleName],
        [PersonMatchedAttribute.LastName],
        [PersonMatchedAttribute.DateOfBirth],
        [PersonMatchedAttribute.EmailAddress],
        [PersonMatchedAttribute.NationalInsuranceNumber],
        [PersonMatchedAttribute.Gender]
    ];

    [Test]
    [MethodDataSource(nameof(GetHighlightedDifferencesData))]
    public async Task Get_HighlightsDifferencesBetweenMatchAndTrnRequest(IReadOnlyCollection<PersonMatchedAttribute> matchedAttributes)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t
                .WithMatchedPersons(matchedPerson.PersonId)
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
                        ? matchedPerson.EmailAddress
                        : TestData.GenerateUniqueEmail())
                .WithNationalInsuranceNumber(
                    matchedAttributes.Contains(PersonMatchedAttribute.NationalInsuranceNumber)
                        ? matchedPerson.NationalInsuranceNumber
                        : TestData.GenerateChangedNationalInsuranceNumber(matchedPerson.NationalInsuranceNumber!))
                .WithGender(
                    matchedAttributes.Contains(PersonMatchedAttribute.Gender)
                        ? matchedPerson.Gender
                        : TestData.GenerateChangedGender(matchedPerson.Gender!)));

        var journeyInstance = await CreateJourneyInstance(supportTask, useFactory: false);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        AssertMatchRowHasExpectedHighlight("Email address", !matchedAttributes.Contains(PersonMatchedAttribute.EmailAddress));
        AssertMatchRowHasExpectedHighlight("NI number", !matchedAttributes.Contains(PersonMatchedAttribute.NationalInsuranceNumber));
        AssertMatchRowHasExpectedHighlight("Gender", !matchedAttributes.Contains(PersonMatchedAttribute.Gender));

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

    [Test]
    public async Task Post_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithStatus(SupportTaskStatus.Closed));

        var personId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId;

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "PersonId", personId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_SubmittedPersonIdIsNotValid_ReturnsBadRequest()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var unmatchedPerson = await TestData.CreatePersonAsync();
        var personId = unmatchedPerson.PersonId;

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "PersonId", personId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_NoChosenPersonId_ReturnsError()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "PersonId", "Select a record");
    }

    [Test]
    public async Task Post_ValidPersonIdChosen_UpdatesStateAndRedirects()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var personId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId;

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "PersonId", personId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(personId, journeyInstance.State.PersonId);
    }

    [Test]
    public async Task Post_CreateNewRecordChosen_UpdatesStateAndRedirects()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var personId = ResolveApiTrnRequestState.CreateNewRecordPersonIdSentinel;

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "PersonId", personId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(personId, journeyInstance.State.PersonId);
    }

    [Test]
    public async Task Post_PersonIdChangedFromState_ClearsPersonAttributes()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var selectedPersonId = ResolveApiTrnRequestState.CreateNewRecordPersonIdSentinel;

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId,
                FirstNameSource = ResolveApiTrnRequestState.PersonAttributeSource.ExistingRecord,
                MiddleNameSource = ResolveApiTrnRequestState.PersonAttributeSource.ExistingRecord,
                LastNameSource = ResolveApiTrnRequestState.PersonAttributeSource.ExistingRecord,
                DateOfBirthSource = ResolveApiTrnRequestState.PersonAttributeSource.ExistingRecord,
                EmailAddressSource = ResolveApiTrnRequestState.PersonAttributeSource.ExistingRecord,
                NationalInsuranceNumberSource = ResolveApiTrnRequestState.PersonAttributeSource.ExistingRecord,
                GenderSource = ResolveApiTrnRequestState.PersonAttributeSource.ExistingRecord,
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "PersonId", selectedPersonId } }
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
        Assert.Null(journeyInstance.State.GenderSource);
        Assert.False(journeyInstance.State.PersonAttributeSourcesSet);
    }

    [Test]
    public async Task Post_PersonIdNotChangedFromState_DoesNotClearPersonAttributes()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var selectedPersonId = ResolveApiTrnRequestState.CreateNewRecordPersonIdSentinel;

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = selectedPersonId,
                FirstNameSource = ResolveApiTrnRequestState.PersonAttributeSource.ExistingRecord,
                MiddleNameSource = ResolveApiTrnRequestState.PersonAttributeSource.ExistingRecord,
                LastNameSource = ResolveApiTrnRequestState.PersonAttributeSource.ExistingRecord,
                DateOfBirthSource = ResolveApiTrnRequestState.PersonAttributeSource.ExistingRecord,
                EmailAddressSource = ResolveApiTrnRequestState.PersonAttributeSource.ExistingRecord,
                NationalInsuranceNumberSource = ResolveApiTrnRequestState.PersonAttributeSource.ExistingRecord,
                GenderSource = ResolveApiTrnRequestState.PersonAttributeSource.ExistingRecord,
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "PersonId", selectedPersonId } }
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
        Assert.NotNull(journeyInstance.State.GenderSource);
        Assert.True(journeyInstance.State.PersonAttributeSourcesSet);
    }

    private async Task<JourneyInstance<ResolveApiTrnRequestState>> CreateJourneyInstance(SupportTask supportTask, bool useFactory = true)
    {
        var state = useFactory
            ? await CreateJourneyStateWithFactory<ResolveApiTrnRequestStateFactory, ResolveApiTrnRequestState>(factory => factory.CreateAsync(supportTask))
            : new ResolveApiTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly()
            };

        return await CreateJourneyInstance(supportTask.SupportTaskReference, state);
    }

    private Task<JourneyInstance<ResolveApiTrnRequestState>> CreateJourneyInstance(
            string supportTaskReference,
            ResolveApiTrnRequestState state) =>
        CreateJourneyInstance(
            JourneyNames.ResolveApiTrnRequest,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));
}
