using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public class CheckAnswersTests : TestBase
{
    private const string _changeReasonDetails = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";

    public CheckAnswersTests(HostFixture hostFixture) : base(hostFixture)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
    }

    public override void Dispose()
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);
        base.Dispose();
    }

    [Fact]
    public async Task Get_PageLegend_PopulatedFromOriginalPersonName()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great"));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("A", "New", "Name")
                .WithChangeReasonChoice(EditDetailsChangeReasonOption.IncompleteDetails)
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var caption = doc.GetElementByTestId("check-answers-caption");
        Assert.Equal("Alfred The Great", caption!.TextContent);
    }

    [Fact]
    public async Task Get_ConfirmAndCancelButtons_ExistOnPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithChangeReasonChoice(EditDetailsChangeReasonOption.IncompleteDetails)
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        var buttons = form.GetElementsByTagName("button").OfType<IHtmlButtonElement>();
        Assert.Collection(buttons,
            b => Assert.Equal("Confirm changes", b.TextContent),
            b => Assert.Equal("Cancel and return to record", b.TextContent));
    }

    public static IEnumerable<object[]> GetInvalidStateData()
    {
        yield return new object[] {
            new EditDetailsStateBuilder()
                .WithInitializedState()
                .WithName(null, "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithChangeReasonChoice(EditDetailsChangeReasonOption.IncompleteDetails)
                .Build()
        };
        yield return new object[] {
            new EditDetailsStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithChangeReasonChoice(EditDetailsChangeReasonOption.AnotherReason)
                .Build()
        };
    }

    [Theory]
    [MemberData(nameof(GetInvalidStateData))]
    public async Task Get_WithInvalidJourneyState_RedirectsToIndex(EditDetailsState state)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/edit-details?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ShowsPersonalDetails_AsExpected()
    {
        // Arrange

        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithEmail("test@test.com")
                .WithMobileNumber("07891 234567")
                .WithNationalInsuranceNumber("AB 12 34 56 C")
                .WithChangeReasonChoice(EditDetailsChangeReasonOption.AnotherReason, _changeReasonDetails)
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListValue("Name", v => Assert.Equal("Alfred The Great", v.TextContent.Trim()));
        doc.AssertSummaryListValue("Teacher reference number (TRN)", v => Assert.Equal(person.Trn!, v.TextContent.Trim()));
        doc.AssertSummaryListValue("Date of birth", v => Assert.Equal("1 February 1980", v.TextContent.Trim()));
        doc.AssertSummaryListValue("Email", v => Assert.Equal("test@test.com", v.TextContent.Trim()));
        doc.AssertSummaryListValue("Mobile number", v => Assert.Equal("07891234567", v.TextContent.Trim()));
        doc.AssertSummaryListValue("National Insurance number", v => Assert.Equal("AB 12 34 56 C", v.TextContent.Trim()));
    }

    [Fact]
    public async Task Get_ShowsMissingOptionalPersonalDetails_AsNotProvided()
    {
        // Arrange

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", null, "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithChangeReasonChoice(EditDetailsChangeReasonOption.AnotherReason, _changeReasonDetails)
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListValue("Name", v => Assert.Equal("Alfred Great", v.TextContent.Trim()));
        doc.AssertSummaryListValue("Teacher reference number (TRN)", v => Assert.Equal("Not provided", v.TextContent.Trim()));
        doc.AssertSummaryListValue("Email", v => Assert.Equal("Not provided", v.TextContent.Trim()));
        doc.AssertSummaryListValue("Mobile number", v => Assert.Equal("Not provided", v.TextContent.Trim()));
        doc.AssertSummaryListValue("National Insurance number", v => Assert.Equal("Not provided", v.TextContent.Trim()));
    }

    [Fact]
    public async Task Get_ShowsChangeReasonAndEvidenceFile_AsExpected()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithChangeReasonChoice(EditDetailsChangeReasonOption.AnotherReason, _changeReasonDetails)
                .WithUploadEvidenceChoice(true, evidenceFileId, "evidence.pdf", "1.2 MB")
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListValue("Reason for changing record", v => Assert.Equal("Another reason", v.TextContent.Trim()));
        doc.AssertSummaryListValue("Reason details", v => Assert.Equal(_changeReasonDetails, v.TextContent.Trim()));
        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";
        doc.AssertSummaryListValue("Do you have evidence to upload", v =>
        {
            var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(v.QuerySelector("a"));
            Assert.Equal("evidence.pdf (opens in new tab)", link.TextContent.Trim());
            Assert.Equal(expectedFileUrl, link.Href);
        });
    }

    [Fact]
    public async Task Get_ShowsMissingAdditionalDetailAndEvidenceFile_AsNotProvided()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithChangeReasonChoice(EditDetailsChangeReasonOption.IncompleteDetails)
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListValue("Reason for changing record", v => Assert.Equal("Data loss or incomplete information", v.TextContent.Trim()));
        doc.AssertSummaryListValue("Reason details", v => Assert.Equal("Not provided", v.TextContent.Trim()));
        doc.AssertSummaryListValue("Do you have evidence to upload", v => Assert.Equal("Not provided", v.TextContent.Trim()));
    }

    public static IEnumerable<object[]> GetFieldChangeData()
    {
        yield return new object[] { new FieldChange[] { new FirstNameChange("Jim") } };
        yield return new object[] { new FieldChange[] { new MiddleNameChange("A") } };
        yield return new object[] { new FieldChange[] { new LastNameChange("Person") } };
        yield return new object[] { new FieldChange[] { new DobChange(DateOnly.Parse("3 July 1990")) } };
        yield return new object[] { new FieldChange[] { new EmailChange("new@email.com") } };
        yield return new object[] { new FieldChange[] { new MobileChange("447654321987") } };
        yield return new object[] { new FieldChange[] { new NinoChange("JK987654D") } };
        yield return new object[] { new FieldChange[] {
            new FirstNameChange("Jim"),
            new MiddleNameChange("A"),
            new LastNameChange("Person"),
            new DobChange(DateOnly.Parse("3 July 1990")),
            new EmailChange("new@email.com"),
            new MobileChange("447654321987"),
            new NinoChange("JK987654D")
        } };
    }

    [Theory]
    [MemberData(nameof(GetFieldChangeData))]
    public async Task Post_Confirm_UpdatesPersonEditDetailsCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(FieldChange[] fieldChanges)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmail("test@test.com")
            .WithMobileNumber("447891234567")
            .WithNationalInsuranceNumber("AB123456C"));

        var firstName = fieldChanges.OfType<FirstNameChange>().FirstOrDefault()?.NewValue ?? person.FirstName;
        var middleName = fieldChanges.OfType<MiddleNameChange>().FirstOrDefault()?.NewValue ?? person.MiddleName;
        var lastName = fieldChanges.OfType<LastNameChange>().FirstOrDefault()?.NewValue ?? person.LastName;
        var dateOfBirth = fieldChanges.OfType<DobChange>().FirstOrDefault()?.NewValue ?? person.DateOfBirth;
        var emailAddress = fieldChanges.OfType<EmailChange>().FirstOrDefault()?.NewValue ?? person.Email;
        var mobileNumber = fieldChanges.OfType<MobileChange>().FirstOrDefault()?.NewValue ?? person.MobileNumber;
        var nationalInsuranceNumber = fieldChanges.OfType<NinoChange>().FirstOrDefault()?.NewValue ?? person.NationalInsuranceNumber;

        var evidenceFileId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState()
                .WithName(firstName, middleName, lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithEmail(emailAddress)
                .WithMobileNumber(mobileNumber)
                .WithNationalInsuranceNumber(nationalInsuranceNumber)
                .WithChangeReasonChoice(EditDetailsChangeReasonOption.AnotherReason, _changeReasonDetails)
                .WithUploadEvidenceChoice(true, evidenceFileId, "evidencefile.png")
                .Build());

        EventPublisher.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}", response.Headers.Location?.OriginalString);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, expectedMessage: "Personal details have been updated successfully.");

        await WithDbContext(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(firstName, updatedPersonRecord.FirstName);
            Assert.Equal(middleName, updatedPersonRecord.MiddleName);
            Assert.Equal(lastName, updatedPersonRecord.LastName);
            Assert.Equal(dateOfBirth, updatedPersonRecord.DateOfBirth);
            Assert.Equal(emailAddress, updatedPersonRecord.EmailAddress);
            Assert.Equal(mobileNumber, updatedPersonRecord.MobileNumber);
            Assert.Equal(nationalInsuranceNumber, updatedPersonRecord.NationalInsuranceNumber);
        });

        var RaisedBy = GetCurrentUserId();

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<PersonDetailsUpdatedEvent>(e);

            Assert.Equal(Clock.UtcNow, actualEvent.CreatedUtc);
            Assert.Equal(person.PersonId, actualEvent.PersonId);
            Assert.Equal(firstName, actualEvent.Details.FirstName);
            Assert.Equal(middleName, actualEvent.Details.MiddleName);
            Assert.Equal(lastName, actualEvent.Details.LastName);
            Assert.Equal(dateOfBirth, actualEvent.Details.DateOfBirth);
            Assert.Equal(emailAddress, actualEvent.Details.EmailAddress);
            Assert.Equal(mobileNumber, actualEvent.Details.MobileNumber);
            Assert.Equal(nationalInsuranceNumber, actualEvent.Details.NationalInsuranceNumber);
            Assert.Equal("Another reason", actualEvent.ChangeReason);
            Assert.Equal(_changeReasonDetails, actualEvent.ChangeReasonDetail);
            Assert.Equal(evidenceFileId, actualEvent.EvidenceFile!.FileId);
            Assert.Equal("evidencefile.png", actualEvent.EvidenceFile.Name);
            var expectedChanges = fieldChanges.Aggregate(PersonDetailsUpdatedEventChanges.None, (acc, c) => acc | c.Change);
            Assert.Equal(expectedChanges, actualEvent.Changes);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, JourneyInstance<EditDetailsState> journeyInstance) =>
        $"/persons/{person.PersonId}/edit-details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<EditDetailsState>> CreateJourneyInstanceAsync(Guid personId, EditDetailsState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditDetails,
            state ?? new EditDetailsState(),
            new KeyValuePair<string, object>("personId", personId));

    public record FieldChange(PersonDetailsUpdatedEventChanges Change);
    public record FirstNameChange(string? NewValue) : FieldChange(PersonDetailsUpdatedEventChanges.FirstName);
    public record MiddleNameChange(string? NewValue) : FieldChange(PersonDetailsUpdatedEventChanges.MiddleName);
    public record LastNameChange(string? NewValue) : FieldChange(PersonDetailsUpdatedEventChanges.LastName);
    public record DobChange(DateOnly? NewValue) : FieldChange(PersonDetailsUpdatedEventChanges.DateOfBirth);
    public record EmailChange(string? NewValue) : FieldChange(PersonDetailsUpdatedEventChanges.EmailAddress);
    public record MobileChange(string? NewValue) : FieldChange(PersonDetailsUpdatedEventChanges.MobileNumber);
    public record NinoChange(string? NewValue) : FieldChange(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber);
}
