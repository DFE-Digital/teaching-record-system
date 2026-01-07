using AngleSharp.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TeacherPensions.Resolve;

public class MatchesTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_PotentialDuplicateTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var taskReference = "1234567";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{taskReference}/resolve/matches");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_PotentialDuplicateTaskIsClosed_ReturnsNotFound()
    {
        // Arrange
        var fileName = "test.txt";
        long integrationTransactionId = 1;
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!));
        var duplicatePerson2 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!));
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithMatchedPersons(duplicatePerson1.PersonId, duplicatePerson2.PersonId);
                s.WithLastName(person.LastName);
                s.WithFirstName(person.FirstName);
                s.WithMiddleName(person.MiddleName);
                s.WithNationalInsuranceNumber(person.NationalInsuranceNumber);
                s.WithGender(person.Gender);
                s.WithDateOfBirth(person.DateOfBirth);
                s.WithSupportTaskData(fileName, integrationTransactionId);
                s.WithCreatedOn(Clock.UtcNow);
                s.WithStatus(SupportTaskStatus.Closed);
            });
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/matches");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ShowsRequestDetails()
    {
        // Arrange
        var fileName = "test.txt";
        long integrationTransactionId = 1;
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithGender());
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithMiddleName(person.MiddleName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!));
        var duplicatePerson2 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithMiddleName(person.MiddleName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!));
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithMatchedPersons(duplicatePerson1.PersonId, duplicatePerson2.PersonId);
                s.WithLastName(person.LastName);
                s.WithFirstName(person.FirstName);
                s.WithMiddleName(person.MiddleName);
                s.WithNationalInsuranceNumber(person.NationalInsuranceNumber);
                s.WithGender(person.Gender);
                s.WithDateOfBirth(person.DateOfBirth);
                s.WithSupportTaskData(fileName, integrationTransactionId);
                s.WithCreatedOn(Clock.UtcNow);
                s.WithStatus(SupportTaskStatus.Open);
            });
        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            [
                new MatchPersonsResultPerson(
                    duplicatePerson1.PersonId,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.NationalInsuranceNumber
                    ]),
                new MatchPersonsResultPerson(
                    duplicatePerson2.PersonId,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.NationalInsuranceNumber
                    ])
             ]);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var requestDetails = doc.GetElementByTestId("request");
        Assert.NotNull(requestDetails);
        Assert.Equal(supportTask.TrnRequestMetadata!.FirstName, requestDetails.GetSummaryListValueByKey("First name"));
        Assert.Equal(supportTask.TrnRequestMetadata!.MiddleName, requestDetails.GetSummaryListValueByKey("Middle name"));
        Assert.Equal(supportTask.TrnRequestMetadata!.LastName, requestDetails.GetSummaryListValueByKey("Last name"));
        Assert.Equal(person.Trn, requestDetails.GetSummaryListValueByKey("TRN"));
        Assert.Equal(supportTask.TrnRequestMetadata!.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), requestDetails.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(supportTask.TrnRequestMetadata!.NationalInsuranceNumber, requestDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(supportTask.TrnRequestMetadata!.Gender?.GetDisplayName(), requestDetails.GetSummaryListValueByKey("Gender"));
    }

    [Fact]
    public async Task Get_ValidRequest_ShowsDetailsOfMatchedRecords()
    {
        // Arrange
        var fileName = "test.txt";
        long integrationTransactionId = 1;
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithGender(Gender.Male));
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithMiddleName(person.MiddleName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!).WithGender(Gender.Male));
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithMatchedPersons(duplicatePerson1.PersonId);
                s.WithLastName(person.LastName);
                s.WithFirstName(person.FirstName);
                s.WithMiddleName(person.MiddleName);
                s.WithNationalInsuranceNumber(person.NationalInsuranceNumber);
                s.WithGender(person.Gender);
                s.WithDateOfBirth(person.DateOfBirth);
                s.WithSupportTaskData(fileName, integrationTransactionId);
                s.WithCreatedOn(Clock.UtcNow);
                s.WithStatus(SupportTaskStatus.Open);
            });
        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            [new MatchPersonsResultPerson(
                duplicatePerson1.PersonId,
                [
                    PersonMatchedAttribute.FirstName,
                    PersonMatchedAttribute.MiddleName,
                    PersonMatchedAttribute.LastName,
                    PersonMatchedAttribute.NationalInsuranceNumber
                ])]);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        Assert.Equal(duplicatePerson1.FirstName, firstMatchDetails.GetSummaryListValueByKey("First name"));
        Assert.Equal(duplicatePerson1.MiddleName, firstMatchDetails.GetSummaryListValueByKey("Middle name"));
        Assert.Equal(duplicatePerson1.LastName, firstMatchDetails.GetSummaryListValueByKey("Last name"));
        Assert.Equal(duplicatePerson1.Trn, firstMatchDetails.GetSummaryListValueByKey("TRN"));
        Assert.Equal(duplicatePerson1.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), firstMatchDetails.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(duplicatePerson1.NationalInsuranceNumber, firstMatchDetails.GetSummaryListValueByKey("NI number"));
        Assert.Equal(duplicatePerson1.Gender?.GetDisplayName(), firstMatchDetails.GetSummaryListValueByKey("Gender"));
    }

    [Fact]
    public async Task Post_NoChosenPersonId_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithGender(Gender.Male));
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!).WithGender(Gender.Male));
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithMatchedPersons(duplicatePerson1.PersonId);
                s.WithLastName(person.LastName);
                s.WithFirstName(person.FirstName);
                s.WithMiddleName(person.MiddleName);
                s.WithNationalInsuranceNumber(person.NationalInsuranceNumber);
                s.WithGender(person.Gender);
                s.WithDateOfBirth(person.DateOfBirth);
                s.WithSupportTaskData("", 0);
                s.WithCreatedOn(Clock.UtcNow);
                s.WithStatus(SupportTaskStatus.Open);
            });

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            [new MatchPersonsResultPerson(
                duplicatePerson1.PersonId,
                [
                    PersonMatchedAttribute.FirstName,
                    PersonMatchedAttribute.MiddleName,
                    PersonMatchedAttribute.LastName,
                    PersonMatchedAttribute.NationalInsuranceNumber
                ])]);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "PersonId", "Select a record");
    }

    public static TheoryData<PersonMatchedAttribute[]> GetHighlightedDifferencesData() => new(
        [PersonMatchedAttribute.FirstName],
        [PersonMatchedAttribute.MiddleName],
        [PersonMatchedAttribute.LastName],
        [PersonMatchedAttribute.DateOfBirth],
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

        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            matchedPerson.PersonId,
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
            [new MatchPersonsResultPerson(
                matchedPerson.PersonId,
                new[]
                {
                    matchedAttributes.Contains(PersonMatchedAttribute.FirstName) ? PersonMatchedAttribute.FirstName : (PersonMatchedAttribute?)null,
                    matchedAttributes.Contains(PersonMatchedAttribute.MiddleName) ? PersonMatchedAttribute.MiddleName : null,
                    matchedAttributes.Contains(PersonMatchedAttribute.LastName) ? PersonMatchedAttribute.LastName : null,
                    matchedAttributes.Contains(PersonMatchedAttribute.DateOfBirth) ? PersonMatchedAttribute.DateOfBirth : null,
                    matchedAttributes.Contains(PersonMatchedAttribute.NationalInsuranceNumber) ? PersonMatchedAttribute.NationalInsuranceNumber : null,
                    matchedAttributes.Contains(PersonMatchedAttribute.Gender) ? PersonMatchedAttribute.Gender : null
                }
                .Where(a => a is not null)
                .Select(a => a!.Value)
                .ToArray()
            )],
            useFactory: false);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "NI number", !matchedAttributes.Contains(PersonMatchedAttribute.NationalInsuranceNumber));
        AssertMatchRowHasExpectedHighlight(firstMatchDetails, "Gender", !matchedAttributes.Contains(PersonMatchedAttribute.Gender));
    }

    public static TheoryData<PersonMatchedAttribute> GetSynonymMatchedData() => new(
        PersonMatchedAttribute.FirstName,
        PersonMatchedAttribute.MiddleName,
        PersonMatchedAttribute.LastName
    );

    [Theory]
    [MemberData(nameof(GetSynonymMatchedData))]
    public async Task Get_WhenMatchedOnSynonymDoesNotHighlightAsDifferenceBetweenMatchAndTrnRequest(PersonMatchedAttribute synonymMatchedAttribute)
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

        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            matchedPerson.PersonId,
            applicationUser.UserId,
            t => t
                .WithMatchedPersons(matchedPerson.PersonId)
                .WithFirstName(
                    synonymMatchedAttribute == PersonMatchedAttribute.FirstName
                        ? synonym
                        : matchedPerson.FirstName)
                .WithMiddleName(
                    synonymMatchedAttribute == PersonMatchedAttribute.MiddleName
                        ? synonym
                        : matchedPerson.MiddleName)
                .WithLastName(
                    synonymMatchedAttribute == PersonMatchedAttribute.LastName
                        ? synonym
                        : matchedPerson.LastName));

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            [new MatchPersonsResultPerson(
                matchedPerson.PersonId,
                [
                    PersonMatchedAttribute.FirstName,
                    PersonMatchedAttribute.MiddleName,
                    PersonMatchedAttribute.LastName
                ]
            )],
            useFactory: false);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithGender(Gender.Male));
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!).WithGender(Gender.Male));
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithMatchedPersons(duplicatePerson1.PersonId);
                s.WithLastName(person.LastName);
                s.WithFirstName(person.FirstName);
                s.WithMiddleName(person.MiddleName);
                s.WithNationalInsuranceNumber(person.NationalInsuranceNumber);
                s.WithGender(person.Gender);
                s.WithDateOfBirth(person.DateOfBirth);
                s.WithSupportTaskData("", 0);
                s.WithCreatedOn(Clock.UtcNow);
                s.WithStatus(SupportTaskStatus.Closed);
            });

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            [new MatchPersonsResultPerson(
                duplicatePerson1.PersonId,
                [
                    PersonMatchedAttribute.FirstName,
                    PersonMatchedAttribute.MiddleName,
                    PersonMatchedAttribute.LastName,
                    PersonMatchedAttribute.NationalInsuranceNumber
                ])]);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "PersonId", person.PersonId } }
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
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithGender(Gender.Male));
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithGender(Gender.Male));
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithMatchedPersons(duplicatePerson1.PersonId);
                s.WithLastName(person.LastName);
                s.WithFirstName(person.FirstName);
                s.WithMiddleName(person.MiddleName);
                s.WithNationalInsuranceNumber(person.NationalInsuranceNumber);
                s.WithGender(person.Gender);
                s.WithDateOfBirth(person.DateOfBirth);
                s.WithSupportTaskData("", 0);
                s.WithCreatedOn(Clock.UtcNow);
                s.WithStatus(SupportTaskStatus.Open);
            });

        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            [new MatchPersonsResultPerson(
                duplicatePerson1.PersonId,
                [
                    PersonMatchedAttribute.FirstName,
                    PersonMatchedAttribute.MiddleName,
                    PersonMatchedAttribute.LastName,
                    PersonMatchedAttribute.NationalInsuranceNumber
                ])]);
        var unmatchedPerson = await TestData.CreatePersonAsync();
        var personId = unmatchedPerson.PersonId;

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "PersonId", personId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_ValidPersonIdChosen_UpdatesStateAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithGender(Gender.Male));
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!).WithGender(Gender.Male));
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithMatchedPersons(duplicatePerson1.PersonId);
                s.WithLastName(person.LastName);
                s.WithFirstName(person.FirstName);
                s.WithMiddleName(person.MiddleName);
                s.WithNationalInsuranceNumber(person.NationalInsuranceNumber);
                s.WithGender(person.Gender);
                s.WithDateOfBirth(person.DateOfBirth);
                s.WithSupportTaskData("", 0);
                s.WithCreatedOn(Clock.UtcNow);
                s.WithStatus(SupportTaskStatus.Open);
            });
        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            [new MatchPersonsResultPerson(
                duplicatePerson1.PersonId,
                [
                    PersonMatchedAttribute.FirstName,
                    PersonMatchedAttribute.MiddleName,
                    PersonMatchedAttribute.LastName,
                    PersonMatchedAttribute.NationalInsuranceNumber
                ])]);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "PersonId", duplicatePerson1.PersonId } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(duplicatePerson1.PersonId, journeyInstance.State.PersonId);
    }

    [Fact]
    public async Task Post_KeepRecordSeparate_UpdatesStateAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithGender(Gender.Male));
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!).WithGender(Gender.Male));
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithMatchedPersons(duplicatePerson1.PersonId);
                s.WithLastName(person.LastName);
                s.WithFirstName(person.FirstName);
                s.WithMiddleName(person.MiddleName);
                s.WithNationalInsuranceNumber(person.NationalInsuranceNumber);
                s.WithGender(person.Gender);
                s.WithDateOfBirth(person.DateOfBirth);
                s.WithSupportTaskData("", 0);
                s.WithCreatedOn(Clock.UtcNow);
                s.WithStatus(SupportTaskStatus.Open);
            });
        var journeyInstance = await CreateJourneyInstance(
            supportTask,
            [new MatchPersonsResultPerson(
                duplicatePerson1.PersonId,
                [
                    PersonMatchedAttribute.FirstName,
                    PersonMatchedAttribute.MiddleName,
                    PersonMatchedAttribute.LastName,
                    PersonMatchedAttribute.NationalInsuranceNumber
                ])]);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "PersonId", Guid.Empty } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/keep-record-separate?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(Guid.Empty, journeyInstance.State.PersonId);
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

    private async Task<JourneyInstance<ResolveTeacherPensionsPotentialDuplicateState>> CreateJourneyInstance(
        SupportTask supportTask,
        MatchPersonsResultPerson[] matchedPersons,
        bool useFactory = true)
    {
        var state = useFactory
            ? await CreateJourneyStateWithFactory<ResolveTeacherPensionsPotentialDuplicateStateFactory, ResolveTeacherPensionsPotentialDuplicateState>(factory => factory.CreateAsync(supportTask))
            : new ResolveTeacherPensionsPotentialDuplicateState
            {
                MatchedPersons = matchedPersons
            };

        return await CreateJourneyInstance(supportTask.SupportTaskReference, state);
    }

    private Task<JourneyInstance<ResolveTeacherPensionsPotentialDuplicateState>> CreateJourneyInstance(
            string supportTaskReference,
            ResolveTeacherPensionsPotentialDuplicateState state) =>
        CreateJourneyInstance(
            JourneyNames.ResolveTpsPotentialDuplicate,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));
}
