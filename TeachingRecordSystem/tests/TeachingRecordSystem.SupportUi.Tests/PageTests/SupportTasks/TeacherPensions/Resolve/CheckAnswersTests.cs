using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;
using TeachingRecordSystem.SupportUi.Services;
using PersonDetailsUpdatedEvent = TeachingRecordSystem.Core.Events.PersonDetailsUpdatedEvent;
using SupportTaskUpdatedEvent = TeachingRecordSystem.Core.Events.SupportTaskUpdatedEvent;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TeacherPensions.Resolve;

public class CheckAnswers : TestBase
{
    public CheckAnswers(HostFixture hostFixture) : base(hostFixture)
    {
        GetAnIdentityApiClientMock
            .Setup(mock => mock.CreateTrnTokenAsync(It.IsAny<CreateTrnTokenRequest>()))
            .ReturnsAsync((CreateTrnTokenRequest req) => new CreateTrnTokenResponse
            {
                Email = req.Email,
                ExpiresUtc = Clock.UtcNow.AddDays(1),
                Trn = req.Trn,
                TrnToken = Guid.NewGuid().ToString()
            });
    }

    [Fact]
    public async Task Get_PotentialDuplicateTaskDoesNotExist_ReturnsNotFound()
    {
        // Arranges
        var taskReference = "1234567";
        var state = new ResolveTeacherPensionsPotentialDuplicateState
        {
            MatchedPersons = []
        };
        var journeyInstance = await CreateJourneyInstance(taskReference, state);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{taskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MergeDetails_ReturnsCorrectContent()
    {
        // Arrange
        var evidenceFileName = "SomeFileName.png";
        var mergeComments = "THIS IS A MERGE RECORD";
        var fileName = "test.txt";
        long integrationTransactionId = 1;
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithGender(Gender.Male));
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithGender(Gender.Female));
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

        var state = new ResolveTeacherPensionsPotentialDuplicateState
        {
            MatchedPersons = [new MatchPersonsResultPerson(duplicatePerson1.PersonId, [])],
            TeachersPensionPersonId = person.PersonId,
            FirstNameSource = PersonAttributeSource.TrnRequest,
            MiddleNameSource = PersonAttributeSource.ExistingRecord,
            LastNameSource = PersonAttributeSource.ExistingRecord,
            DateOfBirthSource = PersonAttributeSource.ExistingRecord,
            GenderSource = PersonAttributeSource.ExistingRecord,
            NationalInsuranceNumberSource = PersonAttributeSource.TrnRequest,
            PersonId = duplicatePerson1.PersonId,
            PersonAttributeSourcesSet = true,
            Evidence = new()
            {
                UploadEvidence = true,
                UploadedEvidenceFile = new()
                {
                    FileId = Guid.NewGuid(),
                    FileName = evidenceFileName,
                    FileSizeDescription = "5MB"
                }
            },
            MergeComments = mergeComments
        };

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/check-answers/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);
        var doc = await AssertEx.HtmlResponseAsync(response);
        var firstName = doc.GetSummaryListValueElementByKey("First name");
        var lastName = doc.GetSummaryListValueElementByKey("Last name");
        var niNumber = doc.GetSummaryListValueElementByKey("NI number");
        var dob = doc.GetSummaryListValueElementByKey("Date of birth");
        var trn = doc.GetSummaryListValueElementByKey("TRN");
        var evidenceFile = doc.GetSummaryListValueElementByKey("Evidence");
        var merge = doc.GetSummaryListValueElementByKey("Comments");

        // Assert
        Assert.NotNull(firstName);
        Assert.NotNull(lastName);
        Assert.NotNull(dob);
        Assert.NotNull(niNumber);
        Assert.NotNull(trn);
        Assert.NotNull(evidenceFile);
        Assert.NotNull(merge);
        Assert.Contains(person.FirstName, firstName.TextContent);
        Assert.Contains(duplicatePerson1.LastName, lastName.TextContent);
        Assert.Contains(duplicatePerson1.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat), dob.TextContent);
        Assert.Contains(person.NationalInsuranceNumber!, niNumber.TextContent);
        Assert.Contains(duplicatePerson1!.Trn, trn.TextContent);
        Assert.Contains(evidenceFileName, evidenceFile.TextContent);
        Assert.Contains(mergeComments, merge.TextContent);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var fileName = "test.txt";
        long integrationTransactionId = 1;
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!));
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

        var state = new ResolveTeacherPensionsPotentialDuplicateState
        {
            MatchedPersons = [new MatchPersonsResultPerson(duplicatePerson1.PersonId, [])],
            TeachersPensionPersonId = person.PersonId,
            DateOfBirthSource = PersonAttributeSource.ExistingRecord,
            FirstNameSource = PersonAttributeSource.ExistingRecord,
            MiddleNameSource = PersonAttributeSource.ExistingRecord,
            LastNameSource = PersonAttributeSource.ExistingRecord,
            GenderSource = PersonAttributeSource.ExistingRecord,
            NationalInsuranceNumberSource = PersonAttributeSource.TrnRequest,
            PersonId = duplicatePerson1.PersonId,
            PersonAttributeSourcesSet = true
        };

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/check-answers/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/support-tasks/teacher-pensions", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Fact]
    public async Task Post_ValidData_ClosesTaskAndRedirectsToTeacherPensionsWithUpdatedDetails()
    {
        // Arrange
        var mergeComments = "merging because evidence has been provided";
        var fileName = "test.txt";
        long integrationTransactionId = 1;
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithGender(Gender.Male));
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithGender(Gender.Female));
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

        var state = new ResolveTeacherPensionsPotentialDuplicateState
        {
            MatchedPersons = [new MatchPersonsResultPerson(duplicatePerson1.PersonId, [])],
            TeachersPensionPersonId = person.PersonId,
            FirstNameSource = PersonAttributeSource.TrnRequest,
            MiddleNameSource = PersonAttributeSource.TrnRequest,
            LastNameSource = PersonAttributeSource.TrnRequest,
            DateOfBirthSource = PersonAttributeSource.TrnRequest,
            GenderSource = PersonAttributeSource.ExistingRecord,
            NationalInsuranceNumberSource = PersonAttributeSource.TrnRequest,
            PersonId = duplicatePerson1.PersonId,
            PersonAttributeSourcesSet = true,
            Evidence = new()
            {
                UploadEvidence = false
            },
            MergeComments = mergeComments
        };

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);
        EventObserver.Clear();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/support-tasks/teacher-pensions", response.Headers.Location?.OriginalString);
        await WithDbContextAsync(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons.SingleAsync(x => x.PersonId == duplicatePerson1.PersonId);
            Assert.Equal(person.FirstName, updatedPersonRecord.FirstName);
            Assert.Equal(person.LastName, updatedPersonRecord.LastName);
            Assert.Equal(person.NationalInsuranceNumber, updatedPersonRecord.NationalInsuranceNumber);
            Assert.Equal(person.DateOfBirth, updatedPersonRecord.DateOfBirth);

            var updatedSupportTask = await dbContext
                .SupportTasks
                .Include(st => st.TrnRequestMetadata)
                .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
        });

        EventObserver.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent>(e);
            Assert.NotNull(actualEvent);
            Assert.Equal(mergeComments, actualEvent.Comments);
            Assert.True(actualEvent.Changes.HasFlag(TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonNameChange));
            Assert.True(actualEvent.Changes.HasFlag(TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonFirstName));
            Assert.True(actualEvent.Changes.HasFlag(TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonLastName));
            Assert.True(actualEvent.Changes.HasFlag(TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonDateOfBirth));
            Assert.True(actualEvent.Changes.HasFlag(TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonNationalInsuranceNumber));
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.TeacherPensionsDuplicateSupportTaskResolvingWithMerge, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<PersonDetailsUpdatedEvent, PersonDeactivatedEvent, TrnRequestUpdatedEvent, SupportTaskUpdatedEvent>();
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();

        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            "Teachers’ Pensions duplicate task completed");

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
