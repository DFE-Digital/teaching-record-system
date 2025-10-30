using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TeacherPensions.Resolve;

public class KeepRecordSeparateTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_PotentialDuplicateTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var taskReference = "1234567";
        var state = new ResolveTeacherPensionsPotentialDuplicateState { MatchedPersonIds = [] };
        var journeyInstance = await CreateJourneyInstance(taskReference, state);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{taskReference}/keep-record-separate?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(KeepingRecordSeparateReason.RecordDoesNotMatch, null)]
    [InlineData(KeepingRecordSeparateReason.AnotherReason, "Keeping them separate for some reason")]
    public async Task Post_Reason_RedirectsToConfirm(KeepingRecordSeparateReason reason, string? additionalComments)
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
                s.WithStatus(SupportTaskStatus.Open);
            });

        var state = new ResolveTeacherPensionsPotentialDuplicateState { MatchedPersonIds = [duplicatePerson1.PersonId] };
        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);

        // Act
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/keep-record-separate?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "KeepSeparateReason", $"{reason}" },
                        { "Reason", $"{additionalComments}" }
                    })
        };
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(reason, journeyInstance.State.KeepSeparateReason);
        Assert.Equal(additionalComments, journeyInstance.State.Reason);
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/confirm-keep-record-separate?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_ReasonNotProvided_ReturnsError()
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
                s.WithStatus(SupportTaskStatus.Open);
            });

        var state = new ResolveTeacherPensionsPotentialDuplicateState { MatchedPersonIds = [duplicatePerson1.PersonId] };
        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);

        // Act
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/keep-record-separate?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "KeepSeparateReason", $"{KeepingRecordSeparateReason.AnotherReason}" },
                        { "Reason", $"" }
                    })
        };
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance.State.KeepSeparateReason);
        Assert.Null(journeyInstance.State.Reason);
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "Reason", "Enter Reason");
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
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
                s.WithStatus(SupportTaskStatus.Open);
            });

        var state = new ResolveTeacherPensionsPotentialDuplicateState { MatchedPersonIds = [duplicatePerson1.PersonId] };
        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/keep-record-separate/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/support-tasks/teacher-pensions", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private Task<JourneyInstance<ResolveTeacherPensionsPotentialDuplicateState>> CreateJourneyInstance(
        string supportTaskReference,
        ResolveTeacherPensionsPotentialDuplicateState state) =>
        CreateJourneyInstance(
            JourneyNames.ResolveTpsPotentialDuplicate,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));

}
