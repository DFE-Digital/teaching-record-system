using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve.ResolveTeacherPensionsPotentialDuplicateState;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TeacherPensions.Resolve;

public class MergeTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_PotentialDuplicateTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var taskReference = "1234567";
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithGender(Gender.Female));
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber(Faker.Identification.UkNationalInsuranceNumber()!).WithGender(Gender.Male));
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
                s.WithSupportTaskData("fileName.csv", 1);
                s.WithCreatedOn(Clock.UtcNow);
                s.WithStatus(SupportTaskStatus.Open);
            });
        var journeyInstance = await CreateJourneyInstance(supportTask, null);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{taskReference}/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_AttributesDifferent_ReturnsHighlightedDifferences()
    {
        // Arrange
        var fileName = "test.txt";
        long integrationTransactionId = 1;
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber().WithGender(Gender.Female));
        var duplicatePerson1 = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber(Faker.Identification.UkNationalInsuranceNumber()!).WithGender(Gender.Male));
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

        var state = new ResolveTeacherPensionsPotentialDuplicateState { MatchedPersonIds = [duplicatePerson1.PersonId], PersonId = duplicatePerson1.PersonId };
        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var firstName = doc.GetElementsByName("FirstNameSource");
        var lastName = doc.GetElementsByName("LastNameSource");
        var niNumber = doc.GetElementsByName("NationalInsuranceNumberSource");
        var dob = doc.GetElementsByName("GenderSource");
        var trn = doc.GetElementsByName("TRNSource");
        Assert.Collection(
            firstName,
            fromRequestRadio =>
            {
                Assert.False(fromRequestRadio.IsDisabled());
            },
            fromExistingRecordRadio =>
            {
                Assert.False(fromExistingRecordRadio.IsDisabled());
                Assert.NotEmpty(
                    fromExistingRecordRadio.GetAncestor<IHtmlDivElement>()!.GetElementsByClassName("hods-highlight"));
            });
        Assert.Collection(
            lastName,
            fromRequestRadio =>
            {
                Assert.False(fromRequestRadio.IsDisabled());
            },
            fromExistingRecordRadio =>
            {
                Assert.False(fromExistingRecordRadio.IsDisabled());
                Assert.NotEmpty(
                    fromExistingRecordRadio.GetAncestor<IHtmlDivElement>()!.GetElementsByClassName("hods-highlight"));
            });
        Assert.Collection(
            niNumber,
            fromRequestRadio =>
            {
                Assert.False(fromRequestRadio.IsDisabled());
            },
            fromExistingRecordRadio =>
            {
                Assert.False(fromExistingRecordRadio.IsDisabled());
                Assert.NotEmpty(
                    fromExistingRecordRadio.GetAncestor<IHtmlDivElement>()!.GetElementsByClassName("hods-highlight"));
            });
        Assert.Collection(
            trn,
            fromRequestRadio =>
            {
                Assert.True(fromRequestRadio.IsDisabled());
            },
            fromExistingRecordRadio =>
            {
                Assert.True(fromExistingRecordRadio.IsDisabled());
                Assert.True(trn[1].IsChecked());
            });
    }

    [Test]
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

        var state = new ResolveTeacherPensionsPotentialDuplicateState { MatchedPersonIds = [duplicatePerson1.PersonId], PersonId = duplicatePerson1.PersonId };
        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/merge/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/support-tasks/teacher-pensions", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Test]
    public async Task Post_WithoutSelectingAnswerToUploadEvidence_ReturnsError()
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

        var state = new ResolveTeacherPensionsPotentialDuplicateState { MatchedPersonIds = [duplicatePerson1.PersonId], PersonId = duplicatePerson1.PersonId };
        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "Evidence.UploadEvidence", "" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.UploadEvidence", "Select yes if you want to upload evidence");
    }

    [Test]
    public async Task Post_WithoutSelectingAnyRadioButtons_ReturnsErrors()
    {
        // Arrange
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

        var state = new ResolveTeacherPensionsPotentialDuplicateState { MatchedPersonIds = [duplicatePerson1.PersonId], PersonId = duplicatePerson1.PersonId };
        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "FirstNameSource", "Select a First name");
        await AssertEx.HtmlResponseHasErrorAsync(response, "LastNameSource", "Select a Last name");
        await AssertEx.HtmlResponseHasErrorAsync(response, "DateOfBirthSource", "Select a date of birth");
        await AssertEx.HtmlResponseHasErrorAsync(response, "NationalInsuranceNumberSource", "Select a National Insurance number");
        await AssertEx.HtmlResponseHasErrorAsync(response, "GenderSource", "Select a gender");
    }

    [Test]
    public async Task Post_ValidRequest_UpdatesStateAndRedirectsToCheckAnswers()
    {
        // Arrange
        var mergeComments = "this is being merged because it's incorrect";
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

        var state = new ResolveTeacherPensionsPotentialDuplicateState { MatchedPersonIds = [duplicatePerson1.PersonId], PersonId = duplicatePerson1.PersonId };
        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "DateOfBirthSource", PersonAttributeSource.ExistingRecord },
                { "NationalInsuranceNumberSource", PersonAttributeSource.ExistingRecord },
                { "GenderSource", PersonAttributeSource.ExistingRecord },
                { "FirstNameSource", PersonAttributeSource.ExistingRecord },
                { "LastNameSource", PersonAttributeSource.ExistingRecord },
                { "Evidence.UploadEvidence", false },
                { "MergeComments", mergeComments }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/check-answers", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(PersonAttributeSource.ExistingRecord, journeyInstance.State.FirstNameSource);
        Assert.Equal(PersonAttributeSource.ExistingRecord, journeyInstance.State.LastNameSource);
        Assert.Equal(PersonAttributeSource.ExistingRecord, journeyInstance.State.DateOfBirthSource);
        Assert.Equal(PersonAttributeSource.ExistingRecord, journeyInstance.State.NationalInsuranceNumberSource);
        Assert.Equal(PersonAttributeSource.ExistingRecord, journeyInstance.State.GenderSource);
        Assert.Equal(mergeComments, journeyInstance.State.MergeComments);
        Assert.Equal(false, journeyInstance.State.Evidence.UploadEvidence);
    }

    [Test]
    public async Task Post_ValidRequestWithEvidenceAttachment_UpdatesStateAndRedirectsToCheckAnswers()
    {
        // Arrange
        var mergeComments = "this is being merged because it's incorrect";
        var fileName = "test.txt";
        var evidenceFile = "evidence.png";
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

        var state = new ResolveTeacherPensionsPotentialDuplicateState { MatchedPersonIds = [duplicatePerson1.PersonId], PersonId = duplicatePerson1.PersonId };
        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                uploadEvidence: true,
                dateOfBirthSource: PersonAttributeSource.TrnRequest,
                NationalInsuranceNumberSource: PersonAttributeSource.TrnRequest,
                FirstNameSource: PersonAttributeSource.TrnRequest,
                LastNameSource: PersonAttributeSource.ExistingRecord,
                GenderSource: PersonAttributeSource.TrnRequest,
                MergeComments: mergeComments,
                evidenceFile: (CreateEvidenceFileBinaryContent(), evidenceFile)
            )
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/support-tasks/teacher-pensions/{supportTask.SupportTaskReference}/resolve/check-answers", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(PersonAttributeSource.TrnRequest, journeyInstance.State.FirstNameSource);
        Assert.Equal(PersonAttributeSource.ExistingRecord, journeyInstance.State.LastNameSource);
        Assert.Equal(PersonAttributeSource.TrnRequest, journeyInstance.State.DateOfBirthSource);
        Assert.Equal(PersonAttributeSource.TrnRequest, journeyInstance.State.NationalInsuranceNumberSource);
        Assert.Equal(PersonAttributeSource.TrnRequest, journeyInstance.State.GenderSource);
        Assert.Equal(mergeComments, journeyInstance.State.MergeComments);
        Assert.Equal(true, journeyInstance.State.Evidence.UploadEvidence);
        Assert.Equal(evidenceFile, journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName);
        Assert.NotNull(journeyInstance.State.Evidence.UploadedEvidenceFile!.FileId);
    }

    private static MultipartFormDataContentBuilder CreatePostContent(
            bool? uploadEvidence = null,
            PersonAttributeSource? dateOfBirthSource = null,
            PersonAttributeSource? NationalInsuranceNumberSource = null,
            PersonAttributeSource? FirstNameSource = null,
            PersonAttributeSource? LastNameSource = null,
            PersonAttributeSource? GenderSource = null,
            string? MergeComments = null,
            (HttpContent Content, string FileName)? evidenceFile = null)
    {
        return new MultipartFormDataContentBuilder
        {
            { "dateOfBirthSource", dateOfBirthSource },
            { "NationalInsuranceNumberSource", NationalInsuranceNumberSource },
            { "FirstNameSource", FirstNameSource },
            { "LastNameSource", LastNameSource },
            { "GenderSource", GenderSource },
            { "MergeComments", MergeComments },
            { "Evidence.UploadEvidence", uploadEvidence },
            { "Evidence.EvidenceFile", evidenceFile }
        };
    }

    private async Task<JourneyInstance<ResolveTeacherPensionsPotentialDuplicateState>> CreateJourneyInstance(SupportTask supportTask, Guid? personId)
    {
        var state = await CreateJourneyStateWithFactory<ResolveTeacherPensionsPotentialDuplicateStateFactory, ResolveTeacherPensionsPotentialDuplicateState>(
            factory => factory.CreateAsync(supportTask));
        state.PersonId = personId;

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
