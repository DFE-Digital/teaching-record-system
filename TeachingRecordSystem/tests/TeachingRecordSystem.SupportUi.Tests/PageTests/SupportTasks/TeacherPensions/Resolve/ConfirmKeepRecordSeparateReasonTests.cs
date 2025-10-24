using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TeacherPensions.Resolve;

public class ConfirmKeepRecordSeparateReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_PotentialDuplicateTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var taskReference = "1234567";
        var state = new ResolveTeacherPensionsPotentialDuplicateState { MatchedPersonIds = [] };
        var journeyInstance = await CreateJourneyInstance(taskReference, state);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{taskReference}/resolve/keep-record-separate?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_ReasonProvided_RendersCorrectContent()
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

        var state = new ResolveTeacherPensionsPotentialDuplicateState
        {
            MatchedPersonIds = [duplicatePerson1.PersonId],
            Reason = "THIS IS A DIFFERENT RECORD",
            KeepSeparateReason = KeepingRecordSeparateReason.AnotherReason
        };
        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/confirm-keep-record-separate?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);
        var doc = await AssertEx.HtmlResponseAsync(response);
        var reason = doc.GetSummaryListValueElementByKey("Reason");
        Assert.NotNull(reason);
        Assert.Contains(state.Reason, reason.TextContent);
    }

    [Test]
    public async Task Get_RecordsDoNotMatch_RendersCorrectContent()
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

        var state = new ResolveTeacherPensionsPotentialDuplicateState
        {
            MatchedPersonIds = [duplicatePerson1.PersonId],
            KeepSeparateReason = KeepingRecordSeparateReason.RecordDoesNotMatch,
            Reason = null
        };
        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/confirm-keep-record-separate?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);
        var doc = await AssertEx.HtmlResponseAsync(response);
        var reason = doc.GetSummaryListValueElementByKey("Reason");
        Assert.NotNull(reason);
        Assert.Contains(state.KeepSeparateReason.GetDisplayName()!, reason.TextContent);
    }

    [Test]
    public async Task Post_RecordsDoNotMatch_SuccessfullyClosesSupportTask()
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

        var state = new ResolveTeacherPensionsPotentialDuplicateState { MatchedPersonIds = [duplicatePerson1.PersonId], KeepSeparateReason = KeepingRecordSeparateReason.RecordDoesNotMatch };
        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);
        EventObserver.Clear();

        // Act
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/confirm-keep-record-separate?{journeyInstance.GetUniqueIdQueryParameter()}");
        var response = await HttpClient.SendAsync(request);

        // Assert
        // support task is updated
        await WithDbContext(async dbContext =>
        {
            var updatedSupportTask = await dbContext
                .SupportTasks
                .Include(st => st.TrnRequestMetadata)
                .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
            var supportTaskData = updatedSupportTask.GetData<TeacherPensionsPotentialDuplicateData>();
            Assert.Null(supportTaskData.ResolvedAttributes);
            Assert.Null(supportTaskData.SelectedPersonAttributes);
        });

        EventObserver.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent>(e);
            Assert.Equal(state.KeepSeparateReason.Value.GetDisplayName(), actualEvent.Comments);
            Assert.Equal(Clock.UtcNow, actualEvent.CreatedUtc);
            Assert.Equal(person.PersonId, actualEvent.PersonId);
        });

        // redirect
        Assert.Equal("/support-tasks/teacher-pensions", response.Headers.Location?.OriginalString);
        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();

        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            "Teachers’ Pensions duplicate task completed",
            $"The records were not merged.");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Test]
    public async Task Post_AnotherReasonProvided_SuccessfullyClosesSupportTask()
    {
        // Arrange
        var keepReason = "other record is a different because of xyz";
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

        var state = new ResolveTeacherPensionsPotentialDuplicateState
        {
            MatchedPersonIds = [duplicatePerson1.PersonId],
            KeepSeparateReason = KeepingRecordSeparateReason.AnotherReason,
            Reason = keepReason
        };
        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);
        EventObserver.Clear();

        // Act
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/confirm-keep-record-separate?{journeyInstance.GetUniqueIdQueryParameter()}");
        var response = await HttpClient.SendAsync(request);

        // Assert
        // support task is updated
        await WithDbContext(async dbContext =>
        {
            var updatedSupportTask = await dbContext
                .SupportTasks
                .Include(st => st.TrnRequestMetadata)
                .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
            var supportTaskData = updatedSupportTask.GetData<TeacherPensionsPotentialDuplicateData>();
            Assert.Null(supportTaskData.ResolvedAttributes);
            Assert.Null(supportTaskData.SelectedPersonAttributes);
        });

        EventObserver.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent>(e);
            Assert.Equal(keepReason, actualEvent.Comments);
            Assert.Equal(Clock.UtcNow, actualEvent.CreatedUtc);
            Assert.Equal(person.PersonId, actualEvent.PersonId);
        });

        // redirect
        Assert.Equal("/support-tasks/teacher-pensions", response.Headers.Location?.OriginalString);
        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();

        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            "Teachers’ Pensions duplicate task completed",
            $"The records were not merged.");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    private Task<JourneyInstance<ResolveTeacherPensionsPotentialDuplicateState>> CreateJourneyInstance(
        string supportTaskReference,
        ResolveTeacherPensionsPotentialDuplicateState state) =>
            CreateJourneyInstance(
                JourneyNames.ResolveTpsPotentialDuplicate,
                state,
                new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));

}
