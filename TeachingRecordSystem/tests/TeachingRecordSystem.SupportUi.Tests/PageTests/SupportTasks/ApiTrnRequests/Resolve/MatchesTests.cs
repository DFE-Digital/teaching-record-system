using AngleSharp.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;
using TeachingRecordSystem.SupportUi.Services;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ApiTrnRequests.Resolve;

public class MatchesTests(HostFixture hostFixture) : ResolveApiTrnRequestTestBase(hostFixture)
{
    [Fact]
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

    [Fact]
    public async Task Get_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches");

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
        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, s => s
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithMiddleName(TestData.GenerateMiddleName())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber())
            .WithGender(TestData.GenerateGender()));

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.DateOfBirth
                    ]
                )).ToArray());

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var requestDetails = doc.GetElementByTestId("request");
        Assert.NotNull(requestDetails);
        Assert.Equal(StringHelper.JoinNonEmpty(' ', supportTask.TrnRequestMetadata!.FirstName, supportTask.TrnRequestMetadata!.MiddleName, supportTask.TrnRequestMetadata!.LastName), requestDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(supportTask.TrnRequestMetadata!.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), requestDetails.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(supportTask.TrnRequestMetadata!.EmailAddress, requestDetails.GetSummaryListValueByKey("Email address"));
        Assert.Equal(supportTask.TrnRequestMetadata!.NationalInsuranceNumber, requestDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(supportTask.TrnRequestMetadata!.Gender?.GetDisplayName(), requestDetails.GetSummaryListValueByKey("Gender"));
    }

    [Fact]
    public async Task Get_ValidRequest_ShowsDetailsOfMatchedRecords()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, s => s
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithMiddleName(TestData.GenerateMiddleName())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber())
            .WithGender(TestData.GenerateGender()));

        var firstMatch = await WithDbContextAsync(dbContext => dbContext.Persons.Include(p => p.PreviousNames).SingleAsync(p => p.PersonId == matchedPersonIds[0]));

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth
                    ]
                )).ToArray());

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        var previousName = firstMatch.PreviousNames?.First();
        var expectedPreviousName = firstMatch.PreviousNames is not null ? StringHelper.JoinNonEmpty(' ', previousName!.FirstName, previousName.MiddleName, previousName.LastName) : null;
        Assert.NotNull(firstMatchDetails);
        Assert.Equal(StringHelper.JoinNonEmpty(' ', firstMatch.FirstName, firstMatch.MiddleName, firstMatch.LastName), firstMatchDetails.GetSummaryListValueByKey("Name"));
        if (expectedPreviousName is not null)
        {
            Assert.Equal(expectedPreviousName, firstMatchDetails.GetSummaryListValueByKey("Previous names"));
        }
        else
        {
            Assert.Null(firstMatchDetails.GetSummaryListValueByKey("Previous names"));
        }

        Assert.Equal(firstMatch.DateOfBirth?.ToString(UiDefaults.DateOnlyDisplayFormat), firstMatchDetails.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(firstMatch.EmailAddress, firstMatchDetails.GetSummaryListValueByKey("Email address"));
        Assert.Equal(firstMatch.NationalInsuranceNumber, firstMatchDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(firstMatch.Gender?.GetDisplayName(), firstMatchDetails.GetSummaryListValueByKey("Gender"));
    }

    [Fact]
    public async Task Get_MultipleMatches_ShowsInOrderOfBestMatchFirst()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var firstName = TestData.GenerateFirstName();
        var middleName = TestData.GenerateMiddleName();
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = TestData.GenerateDateOfBirth();
        var emailAddress = TestData.GenerateUniqueEmail();
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var gender = TestData.GenerateGender();

        // This should be the third best match
        var matchedPerson1 = await TestData.CreatePersonAsync(p => p
            .WithFirstName(firstName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth));

        // This should be the best match
        var matchedPerson2 = await TestData.CreatePersonAsync(p => p
            .WithNationalInsuranceNumber(nationalInsuranceNumber));

        // This should be the second best match
        var matchedPerson3 = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(emailAddress));

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, configure =>
        {
            configure.WithFirstName(firstName);
            configure.WithLastName(lastName);
            configure.WithDateOfBirth(dateOfBirth);
            configure.WithEmailAddress(emailAddress);
            configure.WithNationalInsuranceNumber(nationalInsuranceNumber);
            configure.WithMatchedPersons(matchedPerson1.PersonId, matchedPerson2.PersonId, matchedPerson3.PersonId);
        });

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            []);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var matches = doc.GetAllElementsByTestId("match");
        Assert.Equal(3, matches.Count);
        Assert.Equal(matchedPerson2.PersonId, Guid.Parse(matches.ElementAt(0).GetAttribute("data-personid")!));
        Assert.Equal(matchedPerson3.PersonId, Guid.Parse(matches.ElementAt(1).GetAttribute("data-personid")!));
        Assert.Equal(matchedPerson1.PersonId, Guid.Parse(matches.ElementAt(2).GetAttribute("data-personid")!));
    }

    [Fact]
    public async Task Get_MatchedRecordHasActiveAlerts_ShowsAlertsTag()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithAlert());
        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, matchedPerson.Person);

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.Gender
                    ]
                )).ToArray());

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        var tags = firstMatchDetails.GetSummaryListValueElementByKey("Status")?.GetElementsByClassName("govuk-tag").Select(e => e.TrimmedText()) ?? [];
        Assert.Contains("Alerts", tags);
    }

    [Fact]
    public async Task Get_MatchedRecordHasQts_ShowsQtsTag()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithQts());
        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, matchedPerson.Person);

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.Gender
                    ]
                )).ToArray());

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        var tags = firstMatchDetails.GetSummaryListValueElementByKey("Status")?.GetElementsByClassName("govuk-tag").Select(e => e.TrimmedText()) ?? [];
        Assert.Contains("QTS", tags);
    }

    [Fact]
    public async Task Get_MatchedRecordHasEyts_ShowsEytsTag()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithEyts(Clock.Today.AddDays(-1)));
        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, matchedPerson.Person);

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.Gender
                    ]
                )).ToArray());

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        var tags = firstMatchDetails.GetSummaryListValueElementByKey("Status")?.GetElementsByClassName("govuk-tag").Select(e => e.TrimmedText()) ?? [];
        Assert.Contains("EYTS", tags);
    }

    [Fact]
    public async Task Get_PersonIdInState_SelectsChosenRecord()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var firstMatchId = matchedPersonIds[0];

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                MatchOutcome = MatchPersonsResultOutcome.PotentialMatches,
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

    [Fact]
    public async Task Get_CreateNewRecordInState_SelectsCreateNewRecordOption()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                MatchOutcome = MatchPersonsResultOutcome.PotentialMatches,
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

    [Fact]
    public async Task Get_MatchedRecords_NullableFieldsEmptyInRecordAndEmptyInRequest_ShowsNotProvidedNotHighlighted()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var matchedPerson = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(null)
            .WithNationalInsuranceNumber(false)
            .WithGender(false)
            .WithMiddleName(""));

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, configure =>
        {
            configure.WithMiddleName(null);
            configure.WithEmailAddress(null);
            configure.WithNationalInsuranceNumber(null);
            configure.WithGender(null);
            configure.WithMatchedPersons(matchedPerson.PersonId);
        });

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth
                    ]
                )).ToArray(),
            useFactory: false);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        Assert.Equal("Not provided", firstMatchDetails.GetSummaryListValueByKey("Email address"));
        Assert.Equal("Not provided", firstMatchDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal("Not provided", firstMatchDetails.GetSummaryListValueByKey("Gender"));

        AssertMatchRowHasExpectedHighlight("NI number", false);
        AssertMatchRowHasExpectedHighlight("Gender", false);
        AssertMatchRowHasExpectedHighlight("Email address", false);

        void AssertMatchRowHasExpectedHighlight(string summaryListKey, bool expectHighlight)
        {
            var valueElement = firstMatchDetails.GetSummaryListValueElementByKey(summaryListKey);
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

    [Fact]
    public async Task Get_ShowsRefreshedMatchedPersons()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var initialMatchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(
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

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.NationalInsuranceNumber,
                    ]
                )).ToArray());

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

    [Fact]
    public async Task Get_WithDefiniteMatch_ShowsMergeRecordButton()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var firstName = TestData.GenerateFirstName();
        var middleName = TestData.GenerateMiddleName();
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = TestData.GenerateDateOfBirth();
        var emailAddress = TestData.GenerateUniqueEmail();
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();

        var matchedPerson = await TestData.CreatePersonAsync(p =>
        {
            p.WithFirstName(firstName);
            p.WithMiddleName(middleName);
            p.WithLastName(lastName);
            p.WithDateOfBirth(dateOfBirth);
            p.WithEmailAddress(emailAddress);
            p.WithNationalInsuranceNumber(nationalInsuranceNumber);
        });

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t
                .WithMatchedPersons(matchedPerson.PersonId)
                .WithStatus(SupportTaskStatus.Open)
                .WithFirstName(firstName)
                .WithMiddleName(middleName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithGender(matchedPerson.Gender)
                .WithEmailAddress(emailAddress)
                .WithNationalInsuranceNumber(nationalInsuranceNumber)
            );

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.Gender,
                        PersonMatchedAttribute.NationalInsuranceNumber
                    ]
                )).ToArray());

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        Assert.NotEmpty(doc.GetAllElementsByTestId("merge-record-button"));
    }

    [Fact]
    public async Task Get_WithNoMatches_ShowsCreateNewRecordButton()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var firstName = TestData.GenerateFirstName();
        var middleName = TestData.GenerateMiddleName();
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = TestData.GenerateDateOfBirth();
        var emailAddress = TestData.GenerateUniqueEmail();
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();

        var matchedPerson = await TestData.CreatePersonAsync(p =>
        {
            p.WithFirstName(firstName);
            p.WithMiddleName(middleName);
            p.WithLastName(lastName);
            p.WithDateOfBirth(dateOfBirth);
            p.WithEmailAddress(emailAddress);
        });

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t
                .WithMatchedPersons(matchedPerson.PersonId)
                .WithStatus(SupportTaskStatus.Open)
                .WithFirstName(TestData.GenerateChangedFirstName(firstName))
                .WithMiddleName(TestData.GenerateChangedMiddleName(middleName))
                .WithLastName(TestData.GenerateChangedLastName(lastName))
                .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(dateOfBirth))
                .WithEmailAddress(TestData.GenerateUniqueEmail())
            );

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                    Array.Empty<PersonMatchedAttribute>()
                )).ToArray());

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        Assert.NotEmpty(doc.GetAllElementsByTestId("create-new-record-button"));
    }


    public static TheoryData<PersonMatchedAttribute[], bool> GetHighlightedDifferencesData()
    {
        var matches = TrnRequestServiceMatchAttributeCombinations.GetMatchAttributeCombinations()
            .Select(m => (m, !(m.Contains(PersonMatchedAttribute.FirstName) && m.Contains(PersonMatchedAttribute.MiddleName) && m.Contains(PersonMatchedAttribute.LastName))));
        var nonMatches = TrnRequestServiceMatchAttributeCombinations.GetNonMatchAttributeCombinationExamples()
            .Select(nm => (nm, !(nm.Contains(PersonMatchedAttribute.FirstName) && nm.Contains(PersonMatchedAttribute.MiddleName) && nm.Contains(PersonMatchedAttribute.LastName))));
        return new TheoryData<PersonMatchedAttribute[], bool>(matches.ToArray());
    }

    [Theory]
    [MemberData(nameof(GetHighlightedDifferencesData))]
    public async Task Get_HighlightsDifferencesBetweenMatchAndTrnRequest(IReadOnlyCollection<PersonMatchedAttribute> matchedAttributes, bool nameIsHighlighted)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t
                .WithMatchedPersons(matchedPerson.PersonId)
                .WithFirstName(
                    matchedAttributes.Contains(PersonMatchedAttribute.FirstName)
                        ? matchedPerson.FirstName
                        : TestData.GenerateChangedFirstName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithMiddleName(
                    matchedAttributes.Contains(PersonMatchedAttribute.MiddleName)
                        ? matchedPerson.MiddleName
                        : TestData.GenerateChangedMiddleName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithLastName(
                    matchedAttributes.Contains(PersonMatchedAttribute.LastName)
                        ? matchedPerson.LastName
                        : TestData.GenerateChangedLastName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
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

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                     new[]
                     {
                        matchedAttributes.Contains(PersonMatchedAttribute.FirstName) ? PersonMatchedAttribute.FirstName : (PersonMatchedAttribute?)null,
                        matchedAttributes.Contains(PersonMatchedAttribute.MiddleName) ? PersonMatchedAttribute.MiddleName : null,
                        matchedAttributes.Contains(PersonMatchedAttribute.LastName) ? PersonMatchedAttribute.LastName : null,
                        matchedAttributes.Contains(PersonMatchedAttribute.DateOfBirth) ? PersonMatchedAttribute.DateOfBirth : null,
                        matchedAttributes.Contains(PersonMatchedAttribute.EmailAddress) ? PersonMatchedAttribute.EmailAddress : null,
                        matchedAttributes.Contains(PersonMatchedAttribute.NationalInsuranceNumber) ? PersonMatchedAttribute.NationalInsuranceNumber : null,
                        matchedAttributes.Contains(PersonMatchedAttribute.Gender) ? PersonMatchedAttribute.Gender : null
                    }
                    .Where(a => a is not null)
                    .Select(a => a!.Value)
                    .ToArray()
                )).ToArray(),
            useFactory: false);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Name", nameIsHighlighted);
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Date of birth", !matchedAttributes.Contains(PersonMatchedAttribute.DateOfBirth));
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Email address", !matchedAttributes.Contains(PersonMatchedAttribute.EmailAddress));
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "NI number", !matchedAttributes.Contains(PersonMatchedAttribute.NationalInsuranceNumber));
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Gender", !matchedAttributes.Contains(PersonMatchedAttribute.Gender));
    }

    public static TheoryData<PersonMatchedAttribute[], PersonMatchedAttribute> GetSynonymMatchedData()
    {
        var matches = new List<(PersonMatchedAttribute[], PersonMatchedAttribute)> {
            ([PersonMatchedAttribute.FirstName, PersonMatchedAttribute.MiddleName, PersonMatchedAttribute.LastName], PersonMatchedAttribute.FirstName),
            ([PersonMatchedAttribute.FirstName, PersonMatchedAttribute.MiddleName, PersonMatchedAttribute.LastName], PersonMatchedAttribute.MiddleName),
            ([PersonMatchedAttribute.FirstName, PersonMatchedAttribute.MiddleName, PersonMatchedAttribute.LastName], PersonMatchedAttribute.LastName),
        };
        return new TheoryData<PersonMatchedAttribute[], PersonMatchedAttribute>(matches.ToArray());
    }

    [Theory]
    [MemberData(nameof(GetSynonymMatchedData))]
    public async Task Get_WhenMatchedOnSynonymDoesNotHighlightAsDifferenceBetweenMatchAndTrnRequest(IReadOnlyCollection<PersonMatchedAttribute> matchedAttributes, PersonMatchedAttribute synonymMatchedAttribute)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var name = "Andrew";
        var synonym = "Andy";
        var firstName = synonymMatchedAttribute == PersonMatchedAttribute.FirstName ? name : TestData.GenerateFirstName();
        var middleName = synonymMatchedAttribute == PersonMatchedAttribute.MiddleName ? name : TestData.GenerateMiddleName();
        var lastName = synonymMatchedAttribute == PersonMatchedAttribute.LastName ? name : TestData.GenerateLastName();
        var matchedPerson = await TestData.CreatePersonAsync(
            p => p.WithNationalInsuranceNumber());

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t
                .WithMatchedPersons(matchedPerson.PersonId)
                .WithFirstName(
                    matchedAttributes.Contains(PersonMatchedAttribute.FirstName)
                        ? synonymMatchedAttribute == PersonMatchedAttribute.FirstName
                            ? synonym
                            : matchedPerson.FirstName
                        : TestData.GenerateChangedFirstName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithMiddleName(
                    matchedAttributes.Contains(PersonMatchedAttribute.MiddleName)
                        ? synonymMatchedAttribute == PersonMatchedAttribute.MiddleName
                            ? synonym
                            : matchedPerson.MiddleName
                        : TestData.GenerateChangedMiddleName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName]))
                .WithLastName(
                    matchedAttributes.Contains(PersonMatchedAttribute.LastName)
                        ? synonymMatchedAttribute == PersonMatchedAttribute.LastName
                            ? synonym
                            : matchedPerson.LastName
                        : TestData.GenerateChangedLastName([matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName])));

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                     [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.LastName
                    ]
                )).ToArray(),
            useFactory: false);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "First name", false);
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Middle name", false);
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Last name", false);
    }

    [Fact]
    public async Task Post_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithStatus(SupportTaskStatus.Closed));

        var personId = matchedPersonIds[0];

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.Gender,
                    ]
                )).ToArray());

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

    [Fact]
    public async Task Post_SubmittedPersonIdIsNotValid_ReturnsBadRequest()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var unmatchedPerson = await TestData.CreatePersonAsync();
        var personId = unmatchedPerson.PersonId;

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.Gender,
                    ]
                )).ToArray());

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

    [Fact]
    public async Task Post_NoChosenPersonId_ReturnsError()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.Gender,
                    ]
                )).ToArray());

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

    [Fact]
    public async Task Post_ValidPersonIdChosen_UpdatesStateAndRedirects()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var personId = matchedPersonIds[0];

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.Gender,
                    ]
                )).ToArray());

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

    [Fact]
    public async Task Post_CreateNewRecordChosen_UpdatesStateAndRedirects()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var personId = ResolveApiTrnRequestState.CreateNewRecordPersonIdSentinel;

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.Gender,
                    ]
                )).ToArray());

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

    [Fact]
    public async Task Post_PersonIdChangedFromState_ClearsPersonAttributes()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var selectedPersonId = ResolveApiTrnRequestState.CreateNewRecordPersonIdSentinel;

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                PersonId = matchedPersonIds[0],
                FirstNameSource = PersonAttributeSource.ExistingRecord,
                MiddleNameSource = PersonAttributeSource.ExistingRecord,
                LastNameSource = PersonAttributeSource.ExistingRecord,
                DateOfBirthSource = PersonAttributeSource.ExistingRecord,
                EmailAddressSource = PersonAttributeSource.ExistingRecord,
                NationalInsuranceNumberSource = PersonAttributeSource.ExistingRecord,
                GenderSource = PersonAttributeSource.ExistingRecord,
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

    [Fact]
    public async Task Post_PersonIdNotChangedFromState_DoesNotClearPersonAttributes()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var selectedPersonId = ResolveApiTrnRequestState.CreateNewRecordPersonIdSentinel;

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                PersonId = selectedPersonId,
                FirstNameSource = PersonAttributeSource.ExistingRecord,
                MiddleNameSource = PersonAttributeSource.ExistingRecord,
                LastNameSource = PersonAttributeSource.ExistingRecord,
                DateOfBirthSource = PersonAttributeSource.ExistingRecord,
                EmailAddressSource = PersonAttributeSource.ExistingRecord,
                NationalInsuranceNumberSource = PersonAttributeSource.ExistingRecord,
                GenderSource = PersonAttributeSource.ExistingRecord,
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

    private void AssertMatchRowHasExpectedHighlight(IElement matchDetails, string summaryListKey, bool expectHighlight)
    {
        var valueElement = matchDetails.GetSummaryListValueElementByKey(summaryListKey);
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

    private async Task<JourneyInstance<ResolveApiTrnRequestState>> CreateJourneyInstance(
        SupportTask supportTask,
        MatchPersonsResultPerson[] matchedPersons,
        bool useFactory = true)
    {
        var state = useFactory
            ? await CreateJourneyStateWithFactory<ResolveApiTrnRequestStateFactory, ResolveApiTrnRequestState>(factory => factory.CreateAsync(supportTask))
            : new ResolveApiTrnRequestState
            {
                MatchedPersons = matchedPersons
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
