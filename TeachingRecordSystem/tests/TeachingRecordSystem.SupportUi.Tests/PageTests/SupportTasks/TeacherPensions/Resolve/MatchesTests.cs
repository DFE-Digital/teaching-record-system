using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TeacherPensions.Resolve;

public class MatchesTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_PotentialDuplicateTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var taskReference = "1234567";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{taskReference}/matches");

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
                           s.WithMatchedPersons(new[] { duplicatePerson1.PersonId, duplicatePerson2.PersonId });
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
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/matches");

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
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!));
        var duplicatePerson2 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!));
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { duplicatePerson1.PersonId, duplicatePerson2.PersonId });
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
        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        Assert.Equal(supportTask.TrnRequestMetadata!.NationalInsuranceNumber, requestDetails.GetSummaryListValueForKey("National Insurance number"));
        Assert.Equal(supportTask.TrnRequestMetadata!.Gender?.GetDisplayName(), requestDetails.GetSummaryListValueForKey("Gender"));
    }

    [Fact]
    public async Task Get_ValidRequest_ShowsDetailsOfMatchedRecords()
    {
        // Arrange
        var fileName = "test.txt";
        long integrationTransactionId = 1;
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithGender(Gender.Male));
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!).WithGender(Gender.Male));
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
                       person.PersonId,
                       user.UserId,
                       s =>
                       {
                           s.WithMatchedPersons(new[] { duplicatePerson1.PersonId});
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
        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstMatchDetails = doc.GetAllElementsByTestId("match").First();
        Assert.NotNull(firstMatchDetails);
        Assert.Equal(duplicatePerson1.FirstName, firstMatchDetails.GetSummaryListValueForKey("First name"));
        Assert.Equal(duplicatePerson1.MiddleName, firstMatchDetails.GetSummaryListValueForKey("Middle name"));
        Assert.Equal(duplicatePerson1.LastName, firstMatchDetails.GetSummaryListValueForKey("Last name"));
        Assert.Equal(duplicatePerson1.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), firstMatchDetails.GetSummaryListValueForKey("Date of birth"));
        Assert.Equal(duplicatePerson1.NationalInsuranceNumber, firstMatchDetails.GetSummaryListValueForKey("National Insurance number"));
        Assert.Equal(duplicatePerson1.Gender?.GetDisplayName(), firstMatchDetails.GetSummaryListValueForKey("Gender"));
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
                           s.WithMatchedPersons(new[] { duplicatePerson1.PersonId });
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

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "PersonId", "Select a record");
    }

    //get
    //Multiple matches
    //Get_CreateNewRecordInState_SelectsCreateNewRecordOption
    //get_merge
    //get keep record
    //changes are highlighted
    


    //post
    //Post_PersonIdChangedFromState_ClearsPersonAttributes
    //post keep sepereate
    //post merge


    private Task<JourneyInstance<ResolveTeacherPensionsPotentialDuplicateState>> CreateJourneyInstance(
        string supportTaskReference,
        ResolveTeacherPensionsPotentialDuplicateState? state = null) =>
    CreateJourneyInstance(
        JourneyNames.ResolveTpsPotentialDuplicate,
        state ?? new(),
        new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));
}
