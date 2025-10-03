using System.Text.RegularExpressions;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.Create;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Create;

public class CheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private const string ChangeReasonDetails = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";

    [Test]
    public async Task Get_ConfirmAndCancelButtons_ExistOnPage()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithUploadEvidenceChoice(false)
                .WithCreateReasonChoice(CreateReasonOption.MandatoryQualification)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        var buttons = form.GetElementsByTagName("button").OfType<IHtmlButtonElement>();
        Assert.Collection(buttons,
            b => Assert.Equal("Confirm and create record", b.TrimmedText()),
            b => Assert.Equal("Cancel", b.TrimmedText()));
    }

    [Test]
    public async Task Get_ShowsPersonalDetails_AsExpected()
    {
        // Arrange

        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithEmail("test@test.com")
                .WithNationalInsuranceNumber("AB 12 34 56 C")
                .WithGender(Gender.Other)
                .WithCreateReasonChoice(CreateReasonOption.MandatoryQualification)
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertRow("Full name", v => Assert.Equal("Alfred The Great", v.TrimmedText()));
        doc.AssertRow("Date of birth", v => Assert.Equal("1 February 1980", v.TrimmedText()));
        doc.AssertRow("Email address", v => Assert.Equal("test@test.com", v.TrimmedText()));
        doc.AssertRow("National Insurance number", v => Assert.Equal("AB 12 34 56 C", v.TrimmedText()));
        doc.AssertRow("Gender", v => Assert.Equal("Other", v.TrimmedText()));
    }

    [Test]
    public async Task Get_ShowsMissingOptionalPersonalDetails_AsNotProvided()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", null, "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithCreateReasonChoice(CreateReasonOption.MandatoryQualification)
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertRow("Full name", v => Assert.Equal("Alfred Great", v.TrimmedText()));
        doc.AssertRow("Email address", v => Assert.Equal("Not provided", v.TrimmedText()));
        doc.AssertRow("National Insurance number", v => Assert.Equal("Not provided", v.TrimmedText()));
        doc.AssertRow("Gender", v => Assert.Equal("Not provided", v.TrimmedText()));
    }

    [Test]
    public async Task Get_ShowsCreateReasonAndEvidenceFile_AsExpected()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithCreateReasonChoice(CreateReasonOption.AnotherReason, ChangeReasonDetails)
                .WithUploadEvidenceChoice(true, evidenceFileId, "evidence.pdf", "1.2 MB")
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertRow("Reason for creating record", v => Assert.Equal("Another reason", v.TrimmedText()));
        doc.AssertRow("Reason details", v => Assert.Equal(ChangeReasonDetails, v.TrimmedText()));
        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";
        doc.AssertRow("Evidence", v =>
        {
            var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(v.QuerySelector("a"));
            Assert.Equal("evidence.pdf (opens in new tab)", link.TrimmedText());
            Assert.Equal(expectedFileUrl, link.Href);
        });
    }

    [Test]
    public async Task Get_WhenMissingAdditionalDetailAndEvidenceFile_ShowsAsNotProvided()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithCreateReasonChoice(CreateReasonOption.MandatoryQualification)
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertRow("Reason for creating record", v => Assert.Equal("They were awarded a mandatory qualification", v.TrimmedText()));
        doc.AssertRow("Reason details", v => Assert.Equal("Not provided", v.TrimmedText()));
        doc.AssertRows("Evidence", v => Assert.Equal("Not provided", v.TrimmedText()));
    }

    [Test]
    public async Task Post_Confirm_UpdatesPersonEditDetailsCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var firstName = "Alfred";
        var middleName = "The";
        var lastName = "Great";
        var dateOfBirth = DateOnly.Parse("1 Feb 1980");
        var emailAddress = "test@test.com";
        var nationalInsuranceNumber = "AB123456C";
        var gender = Gender.Female;

        var nameEvidenceFileId = Guid.NewGuid();
        var otherEvidenceFileId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName(firstName, middleName, lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithEmail(emailAddress)
                .WithNationalInsuranceNumber(nationalInsuranceNumber)
                .WithGender(gender)
                .WithCreateReasonChoice(CreateReasonOption.AnotherReason, ChangeReasonDetails)
                .WithUploadEvidenceChoice(true, otherEvidenceFileId, "other-evidence.png")
                .Build());

        EventObserver.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var path = new Regex(@"/persons/([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})");
        var match = path.Match(response.Headers.Location?.OriginalString ?? "");
        Assert.True(match.Success);
        var personId = Guid.Parse(match.Groups[1].ToString());

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, expectedHeading: "Record created for Alfred The Great");

        await WithDbContext(async dbContext =>
        {
            var createdPersonRecord = await dbContext.Persons.SingleAsync(p => p.PersonId == personId);
            Assert.Equal(Clock.UtcNow, createdPersonRecord.CreatedOn);
            Assert.Equal(Clock.UtcNow, createdPersonRecord.UpdatedOn);
            Assert.Equal(firstName, createdPersonRecord.FirstName);
            Assert.Equal(middleName, createdPersonRecord.MiddleName);
            Assert.Equal(lastName, createdPersonRecord.LastName);
            Assert.Equal(dateOfBirth, createdPersonRecord.DateOfBirth);
            Assert.Equal(emailAddress, createdPersonRecord.EmailAddress);
            Assert.Equal(nationalInsuranceNumber, createdPersonRecord.NationalInsuranceNumber);
            Assert.Equal(gender, createdPersonRecord.Gender);
        });

        var raisedBy = GetCurrentUserId();

        EventObserver.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<PersonCreatedEvent>(e);

            Assert.Equal(Clock.UtcNow, actualEvent.CreatedUtc);
            Assert.Equal(personId, actualEvent.PersonId);
            Assert.Equal(firstName, actualEvent.PersonAttributes.FirstName);
            Assert.Equal(middleName, actualEvent.PersonAttributes.MiddleName);
            Assert.Equal(lastName, actualEvent.PersonAttributes.LastName);
            Assert.Equal(dateOfBirth, actualEvent.PersonAttributes.DateOfBirth);
            Assert.Equal(emailAddress, actualEvent.PersonAttributes.EmailAddress);
            Assert.Equal(nationalInsuranceNumber, actualEvent.PersonAttributes.NationalInsuranceNumber);
            Assert.Equal(gender, actualEvent.PersonAttributes.Gender);
            Assert.Equal("Another reason", actualEvent.CreateReason);
            Assert.Equal(ChangeReasonDetails, actualEvent.CreateReasonDetail);
            Assert.Equal(otherEvidenceFileId, actualEvent.EvidenceFile!.FileId);
            Assert.Equal("other-evidence.png", actualEvent.EvidenceFile.Name);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    private string GetRequestPath(JourneyInstance<CreateState> journeyInstance) =>
        $"/persons/create/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<CreateState>> CreateJourneyInstanceAsync(CreateState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.CreatePerson,
            state ?? new CreateState());
}
