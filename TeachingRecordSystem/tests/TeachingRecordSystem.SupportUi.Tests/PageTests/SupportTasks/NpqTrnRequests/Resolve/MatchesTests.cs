using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;
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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
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
                PersonId = matchedPersonIds[0],
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
            Assert.Equal($"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/{expectedBackLink}?createRecord=True", doc.GetElementsByClassName("govuk-back-link").Single().GetAttribute("href"));
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
        var (supportTask, _, matchedPersons) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
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
        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, s => s
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
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress
                    ]
                )).ToArray());

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        Assert.Equal($"{supportTask.TrnRequestMetadata!.NpqEvidenceFileName} (opens in new tab)", requestDetails.GetSummaryListValueByKey("Evidence"));
    }

    [Fact]
    public async Task Get_ValidRequest_ShowsDetailsOfMatchedRecords()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");
        var matchedPerson = await TestData.CreatePersonAsync(p => p
            .WithPreviousNames((TestData.GenerateFirstName(), TestData.GenerateMiddleName(), TestData.GenerateLastName(), new DateTime(2020, 1, 1).ToUniversalTime()))
            .WithEmailAddress(TestData.GenerateUniqueEmail())
            .WithNationalInsuranceNumber()
            .WithGender(TestData.GenerateGender()));
        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, configure => configure.WithMatchedPersons(matchedPerson.PersonId));

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            matchedPersonIds.Select(
                p => new MatchPersonsResultPerson(
                    p,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth
                    ]
                )).ToArray(),
            useFactory: false);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        Assert.Equal(StringHelper.JoinNonEmpty(' ', matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName), firstMatchDetails.GetSummaryListValueByKey("Name"));
        Assert.Equal(StringHelper.JoinNonEmpty(' ', matchedPerson.PreviousNames.First().FirstName, matchedPerson.PreviousNames.First().MiddleName, matchedPerson.PreviousNames.First().LastName), firstMatchDetails.GetSummaryListValueByKey("Previous names"));
        Assert.Equal(matchedPerson.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), firstMatchDetails.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(matchedPerson.EmailAddress, firstMatchDetails.GetSummaryListValueByKey("Email address"));
        Assert.Equal(matchedPerson.NationalInsuranceNumber, firstMatchDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(matchedPerson.Gender?.GetDisplayName(), firstMatchDetails.GetSummaryListValueByKey("Gender"));
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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, configure =>
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
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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
    public async Task Get_MatchedRecords_NullableFieldsEmptyInRecordButPopulatedInRequest_ShowsHighlightedNotProvided()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var matchedPerson = await TestData.CreatePersonAsync(p => p
            .WithNationalInsuranceNumber(false)
            .WithGender(false));

        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, configure =>
        {
            configure.WithLastName(TestData.GenerateChangedLastName(matchedPerson.LastName));
            configure.WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
            configure.WithGender(TestData.GenerateGender());
            configure.WithEmailAddress(TestData.GenerateUniqueEmail());
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
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        Assert.Equal(StringHelper.JoinNonEmpty(' ', matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName), firstMatchDetails.GetSummaryListValueByKey("Name"));

        Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("Gender"));

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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, configure =>
        {
            configure.WithMiddleName(null);
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
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress
                    ]
                )).ToArray(),
            useFactory: false);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, firstMatchDetails.GetSummaryListValueByKey("Gender"));

        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "NI number", false);
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Gender", false);
    }

    [Fact]
    public async Task Get_ShowsRefreshedMatchedPersons()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var initialMatchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());

        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
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
                        PersonMatchedAttribute.NationalInsuranceNumber
                    ]
                )).ToArray());

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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
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
                        PersonMatchedAttribute.NationalInsuranceNumber,
                        PersonMatchedAttribute.Gender
                    ]
                )).ToArray());

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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, configure =>
        {
            configure.WithMiddleName("John");
            configure.WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
            configure.WithEmailAddress("something+different@education.gov.uk");
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
        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, matchedPerson.Person);

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
        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, matchedPerson.Person);

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
        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, matchedPerson.Person);

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
        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var firstMatchId = matchedPersonIds[0];

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
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
        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
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
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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
    public async Task Get_WhenMatchedOnSynonymDoesNotHighlightAsDifferenceBetweenMatchAndTrnRequest(PersonMatchedAttribute[] matchedAttributes, PersonMatchedAttribute synonymMatchedAttribute)
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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
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
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Name", false);
    }

    [Fact]
    public async Task Post_TaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
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
        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

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
                        PersonMatchedAttribute.Gender
                    ]
                )).ToArray());

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
        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

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
        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

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
                        PersonMatchedAttribute.Gender
                    ]
                )).ToArray());

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
        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var personId = ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel;

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
        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var selectedPersonId = ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel;

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
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
        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var selectedPersonId = ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel;

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
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
        var (supportTask, _, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var selectedPersonId = ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel;

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
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

    private async Task<JourneyInstance<ResolveNpqTrnRequestState>> CreateJourneyInstance(
        SupportTask supportTask,
        MatchPersonsResultPerson[] matchedPersons,
        bool useFactory = true)
    {
        var state = useFactory
            ? await CreateJourneyStateWithFactory<ResolveNpqTrnRequestStateFactory, ResolveNpqTrnRequestState>(factory => factory.CreateAsync(supportTask))
            : new ResolveNpqTrnRequestState
            {
                MatchedPersons = matchedPersons
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
