using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TeacherPensions.Resolve;

public class KeepRecordSeparateTests(HostFixture hostFixture) : ResolveTeacherPensionsPotentialDuplicateTestBase(hostFixture)
{
    [Fact]
    public async Task Get_PotentialDuplicateTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var taskReference = "1234567";
        var state = new ResolveTeacherPensionsPotentialDuplicateState { MatchedPersons = [], PersonId = ResolveTeacherPensionsPotentialDuplicateState.KeepRecordSeparatePersonIdSentinel };
        var journeyInstance = await CreateJourneyInstanceAsync(taskReference, state);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{taskReference}/resolve/keep-record-separate?{journeyInstance.GetUniqueIdQueryParameter()}");

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
                s.WithCreatedOn(TimeProvider.UtcNow);
                s.WithStatus(SupportTaskStatus.Open);
            });

        var state = new ResolveTeacherPensionsPotentialDuplicateState
        {
            MatchedPersons = [new MatchPersonsResultPerson(duplicatePerson1.PersonId, [])],
            PersonId = ResolveTeacherPensionsPotentialDuplicateState.KeepRecordSeparatePersonIdSentinel
        };
        var journeyInstance = await CreateJourneyInstanceAsync(supportTask.SupportTaskReference, state);

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
        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.Equal(reason, journeyState!.KeepSeparateReason);
        Assert.Equal(additionalComments, journeyState!.Reason);
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
                s.WithCreatedOn(TimeProvider.UtcNow);
                s.WithStatus(SupportTaskStatus.Open);
            });

        var state = new ResolveTeacherPensionsPotentialDuplicateState
        {
            MatchedPersons = [new MatchPersonsResultPerson(duplicatePerson1.PersonId, [])],
            PersonId = ResolveTeacherPensionsPotentialDuplicateState.KeepRecordSeparatePersonIdSentinel
        };
        var journeyInstance = await CreateJourneyInstanceAsync(supportTask.SupportTaskReference, state);

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
        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.Null(journeyState!.KeepSeparateReason);
        Assert.Null(journeyState!.Reason);
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
                s.WithCreatedOn(TimeProvider.UtcNow);
                s.WithStatus(SupportTaskStatus.Open);
            });

        var state = new ResolveTeacherPensionsPotentialDuplicateState
        {
            MatchedPersons = [new MatchPersonsResultPerson(duplicatePerson1.PersonId, [])],
            PersonId = ResolveTeacherPensionsPotentialDuplicateState.KeepRecordSeparatePersonIdSentinel
        };
        var journeyInstance = await CreateJourneyInstanceAsync(supportTask.SupportTaskReference, state);
        var pageUrl = $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/keep-record-separate?{journeyInstance.GetUniqueIdQueryParameter()}";

        // Submit the Cancel button exactly as the rendered page defines it, rather than a URL of our
        // own choosing — this button used to post to the check answers page, which threw.
        var doc = await AssertEx.HtmlResponseAsync(await HttpClient.GetAsync(pageUrl));
        var cancelButton = doc.GetElementsByTagName("button").Single(b => b.TextContent.Trim() == "Cancel");
        Assert.Null(cancelButton.GetAttribute("formaction"));

        var request = new HttpRequestMessage(HttpMethod.Post, pageUrl)
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { cancelButton.GetAttribute("name")!, cancelButton.GetAttribute("value")! }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks/teacher-pensions", response.Headers.Location?.OriginalString);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }


}
