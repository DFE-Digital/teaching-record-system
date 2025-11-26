using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;
using TeachingRecordSystem.SupportUi.Services;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests.Resolve;

public class MatchesTests(HostFixture hostFixture) : NpqTrnRequestTestBase(hostFixture)
{
    [Theory]
    [InlineData(false, "details")]
    [InlineData(true, "resolve/check-answers")]
    public async Task Get_HasExpectedBackLink(bool fromCheckAnswers, string expectedBackLink)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                MatchOutcome = TrnRequestMatchResultOutcome.PotentialMatches,
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId,
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}" + (fromCheckAnswers ? "&fromCheckAnswers=true" : ""));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        if (fromCheckAnswers)
        {
            Assert.Equal($"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/{expectedBackLink}?{journeyInstance.GetUniqueIdQueryParameter()}", doc.GetElementsByClassName("govuk-back-link").Single().GetAttribute("href"));
        }
        else
        {
            Assert.Equal($"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/{expectedBackLink}", doc.GetElementsByClassName("govuk-back-link").Single().GetAttribute("href"));
        }

    }

    [Fact]
    public async Task Get_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var taskReference = SupportTask.GenerateSupportTaskReference();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/npq-trn-requests/{taskReference}/resolve/matches");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches");

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
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, s => s
            .WithMiddleName(TestData.GenerateMiddleName())
            .WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber())
            .WithGender(TestData.GenerateGender()));

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var requestDetails = doc.GetElementByTestId("request");
        Assert.NotNull(requestDetails);
        Assert.Equal(supportTask.TrnRequestMetadata!.FirstName, requestDetails.GetSummaryListValueByKey("First name"));
        Assert.Equal(supportTask.TrnRequestMetadata!.MiddleName, requestDetails.GetSummaryListValueByKey("Middle name"));
        Assert.Equal(supportTask.TrnRequestMetadata!.LastName, requestDetails.GetSummaryListValueByKey("Last name"));
        Assert.Equal(supportTask.TrnRequestMetadata!.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), requestDetails.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(supportTask.TrnRequestMetadata!.EmailAddress, requestDetails.GetSummaryListValueByKey("Email address"));
        Assert.Equal(supportTask.TrnRequestMetadata!.NationalInsuranceNumber, requestDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(supportTask.TrnRequestMetadata!.Gender?.GetDisplayName(), requestDetails.GetSummaryListValueByKey("Gender"));
        Assert.Equal($"{supportTask.TrnRequestMetadata!.NpqEvidenceFileName} (opens in new tab)", requestDetails.GetSummaryListValueByKey("Evidence"));
    }

    [Fact]
    public async Task Get_ValidRequest_ShowsDetailsOfMatchedRecords()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");
        var matchedPerson = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber()
            .WithGender(TestData.GenerateGender()));
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, configure => configure.WithMatchedPersons(matchedPerson.PersonId));

        var journeyInstance = await CreateJourneyInstance(supportTask, useFactory: false);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        Assert.Equal(matchedPerson.FirstName, firstMatchDetails.GetSummaryListValueByKey("First name"));
        Assert.Equal(matchedPerson.MiddleName, firstMatchDetails.GetSummaryListValueByKey("Middle name"));
        Assert.Equal(matchedPerson.LastName, firstMatchDetails.GetSummaryListValueByKey("Last name"));
        Assert.Equal(matchedPerson.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), firstMatchDetails.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(matchedPerson.EmailAddress, firstMatchDetails.GetSummaryListValueByKey("Email address"));
        Assert.Equal(matchedPerson.NationalInsuranceNumber, firstMatchDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(matchedPerson.Gender?.GetDisplayName(), firstMatchDetails.GetSummaryListValueByKey("Gender"));
    }

    [Fact]
    public async Task Get_MatchedRecords_NullableFieldsEmptyInRecordButPopulatedInRequest_ShowsHighlightedNotProvided()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var matchedPerson = await TestData.CreatePersonAsync(p => p
            .WithNationalInsuranceNumber(false)
            .WithGender(false)
            .WithMiddleName(""));

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, configure =>
        {
            configure.WithMiddleName("John");
            configure.WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
            configure.WithGender(TestData.GenerateGender());
            configure.WithEmailAddress(TestData.GenerateUniqueEmail());
            configure.WithMatchedPersons(matchedPerson.PersonId);
        });

        var journeyInstance = await CreateJourneyInstance(supportTask, useFactory: false);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        Assert.Equal(matchedPerson.FirstName, firstMatchDetails.GetSummaryListValueByKey("First name"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("Middle name"));
        Assert.Equal(matchedPerson.LastName, firstMatchDetails.GetSummaryListValueByKey("Last name"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("Gender"));

        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Middle name", true);
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "NI number", true);
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Gender", true);
    }

    [Fact]
    public async Task Get_MatchedRecords_NullableFieldsEmptyInRecordAndEmptyInRequest_ShowsNotProvidedNotHighlighted
    ()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var matchedPerson = await TestData.CreatePersonAsync(p => p
            .WithNationalInsuranceNumber(false)
            .WithGender(false)
            .WithMiddleName(""));

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, configure =>
        {
            configure.WithMiddleName(null);
            configure.WithNationalInsuranceNumber(null);
            configure.WithGender(null);
            configure.WithMatchedPersons(matchedPerson.PersonId);
        });

        var journeyInstance = await CreateJourneyInstance(supportTask, useFactory: false);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        Assert.Equal(matchedPerson.FirstName, firstMatchDetails.GetSummaryListValueByKey("First name"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("Middle name"));
        Assert.Equal(matchedPerson.LastName, firstMatchDetails.GetSummaryListValueByKey("Last name"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("Gender"));

        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Middle name", false);
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "NI number", false);
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Gender", false);
    }

    [Fact]
    public async Task Get_ShowsRefreshedMatchedPersons()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var initialMatchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
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
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
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

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
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

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        Assert.NotEmpty(doc.GetAllElementsByTestId("create-new-record-button"));
    }

    [Fact]
    public async Task Get_MatchedRecords_EmailRenderedAsExpected()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var matchedPerson = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress("something+test@education.gov.uk"));

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, configure =>
        {
            configure.WithMiddleName("John");
            configure.WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
            configure.WithEmailAddress("something+different@education.gov.uk");
            configure.WithMatchedPersons(matchedPerson.PersonId);
        });

        var journeyInstance = await CreateJourneyInstance(supportTask, useFactory: false);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        Assert.Equal(matchedPerson.EmailAddress, firstMatchDetails.GetSummaryListValueByKey("Email address"));
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Email address", true);
    }

    [Fact]
    public async Task Get_MatchedRecordHasActiveAlerts_ShowsAlertsTag()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithAlert().WithEmailAddress());
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, matchedPerson.Person);

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithQts().WithEmailAddress());
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, matchedPerson.Person);

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithEyts().WithEmailAddress());
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, matchedPerson.Person);

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var firstMatchId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId;

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                MatchOutcome = TrnRequestMatchResultOutcome.PotentialMatches,
                PersonId = firstMatchId
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                MatchOutcome = TrnRequestMatchResultOutcome.PotentialMatches,
                PersonId = ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        Assert.True(
            doc.GetElementsByName("PersonId")
                .Single(e => e.GetAttribute("value") == ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel.ToString())
                .IsChecked());
    }

    public static TheoryData<PersonMatchedAttribute[]> GetHighlightedDifferencesData() => new(
        [PersonMatchedAttribute.FirstName],
        [PersonMatchedAttribute.MiddleName],
        [PersonMatchedAttribute.LastName],
        [PersonMatchedAttribute.DateOfBirth],
        [PersonMatchedAttribute.EmailAddress],
        [PersonMatchedAttribute.NationalInsuranceNumber],
        [PersonMatchedAttribute.Gender]
    );

    [Theory]
    [MemberData(nameof(GetHighlightedDifferencesData))]
    public async Task Get_HighlightsDifferencesBetweenMatchAndTrnRequest(IReadOnlyCollection<PersonMatchedAttribute> matchedAttributes)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
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
                        ? matchedPerson.EmailAddress!
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
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "First name", !matchedAttributes.Contains(PersonMatchedAttribute.FirstName));
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Middle name", !matchedAttributes.Contains(PersonMatchedAttribute.MiddleName));
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Last name", !matchedAttributes.Contains(PersonMatchedAttribute.LastName));
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Date of birth", !matchedAttributes.Contains(PersonMatchedAttribute.DateOfBirth));
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Email address", !matchedAttributes.Contains(PersonMatchedAttribute.EmailAddress));
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "NI number", !matchedAttributes.Contains(PersonMatchedAttribute.NationalInsuranceNumber));
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Gender", !matchedAttributes.Contains(PersonMatchedAttribute.Gender));
    }

    [Fact]
    public async Task Post_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t.WithStatus(SupportTaskStatus.Closed));

        var personId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId;

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var unmatchedPerson = await TestData.CreatePersonAsync();
        var personId = unmatchedPerson.PersonId;

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
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

        var personId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId;

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "PersonId", personId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
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

        var journeyInstance = await CreateJourneyInstance(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "PersonId", personId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}",
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
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId,
                DateOfBirthSource = PersonAttributeSource.ExistingRecord,
                EmailAddressSource = PersonAttributeSource.ExistingRecord,
                NationalInsuranceNumberSource = PersonAttributeSource.ExistingRecord,
                GenderSource = PersonAttributeSource.ExistingRecord,
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "PersonId", selectedPersonId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
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
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var selectedPersonId = ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel;

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = selectedPersonId,
                DateOfBirthSource = PersonAttributeSource.ExistingRecord,
                EmailAddressSource = PersonAttributeSource.ExistingRecord,
                NationalInsuranceNumberSource = PersonAttributeSource.ExistingRecord,
                GenderSource = PersonAttributeSource.ExistingRecord,
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "PersonId", selectedPersonId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.NotNull(journeyInstance.State.DateOfBirthSource);
        Assert.NotNull(journeyInstance.State.EmailAddressSource);
        Assert.NotNull(journeyInstance.State.NationalInsuranceNumberSource);
        Assert.NotNull(journeyInstance.State.GenderSource);
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
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = selectedPersonId,
                DateOfBirthSource = PersonAttributeSource.ExistingRecord,
                EmailAddressSource = PersonAttributeSource.ExistingRecord,
                NationalInsuranceNumberSource = PersonAttributeSource.ExistingRecord,
                GenderSource = PersonAttributeSource.ExistingRecord,
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        Assert.Equal($"/support-tasks/npq-trn-requests", location);
        Assert.Null(await ReloadJourneyInstance(journeyInstance));
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

    private async Task<JourneyInstance<ResolveNpqTrnRequestState>> CreateJourneyInstance(SupportTask supportTask, bool useFactory = true)
    {
        var state = useFactory
            ? await CreateJourneyStateWithFactory<ResolveNpqTrnRequestStateFactory, ResolveNpqTrnRequestState>(factory => factory.CreateAsync(supportTask))
            : new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly()
            };

        return await CreateJourneyInstance(supportTask.SupportTaskReference, state);
    }

    private Task<JourneyInstance<ResolveNpqTrnRequestState>> CreateJourneyInstance(
            string supportTaskReference,
            ResolveNpqTrnRequestState state) =>
        CreateJourneyInstance(
            JourneyNames.ResolveNpqTrnRequest,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));
}
