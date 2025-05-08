using AngleSharp.Html.Dom;
using Newtonsoft.Json.Linq;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public class CheckAnswersTests : TestBase
{
    private const string _changeReasonDetails = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";

    public CheckAnswersTests(HostFixture hostFixture) : base(hostFixture)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.NewPersonDetails);
    }

    public override void Dispose()
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.NewPersonDetails);
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
                .WithInitializedState("A", "New", "Name", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
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
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
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
                .WithInitializedState(null, "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .WithChangeReasonChoice(EditDetailsChangeReasonOption.IncompleteDetails)
                .Build()
        };
        yield return new object[] {
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
                .WithChangeReasonChoice(EditDetailsChangeReasonOption.AnotherReason)
                .Build()
        };
    }

    [Theory]
    [MemberData(nameof(GetInvalidStateData))]
    public async Task Get_WithInvalidJourneyState_RedirectsToStart(EditDetailsState editEditDetailsState)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editEditDetailsState);

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
        var editEditDetailsState = new EditDetailsStateBuilder()
            .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
            .WithChangeReasonChoice(EditDetailsChangeReasonOption.AnotherReason, _changeReasonDetails)
            .WithUploadEvidenceChoice(false)
            .Build();

        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editEditDetailsState);

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        AssertSummaryListValue(doc, "Name", v => Assert.Equal("Alfred The Great", v.TextContent.Trim()));
        AssertSummaryListValue(doc, "Teacher reference number (TRN)", v => Assert.Equal(person.Trn!, v.TextContent.Trim()));
        AssertSummaryListValue(doc, "Date of birth", v => Assert.Equal("1 February 1980", v.TextContent.Trim()));
        AssertSummaryListValue(doc, "Email", v => Assert.Equal("test@test.com", v.TextContent.Trim()));
        AssertSummaryListValue(doc, "Mobile number", v => Assert.Equal("07891234567", v.TextContent.Trim()));
        AssertSummaryListValue(doc, "National Insurance number", v => Assert.Equal("AB 12 34 56 C", v.TextContent.Trim()));
    }

    [Fact]
    public async Task Get_ShowsMissingOptionalPersonalDetails_AsNotProvided()
    {
        // Arrange
        var editEditDetailsState = new EditDetailsStateBuilder()
            .WithInitializedState("Alfred", null, "Great", DateOnly.Parse("1 Feb 1980"), null, null, null)
            .WithChangeReasonChoice(EditDetailsChangeReasonOption.AnotherReason, _changeReasonDetails)
            .WithUploadEvidenceChoice(false)
            .Build();

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editEditDetailsState);

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        AssertSummaryListValue(doc, "Name", v => Assert.Equal("Alfred Great", v.TextContent.Trim()));
        AssertSummaryListValue(doc, "Teacher reference number (TRN)", v => Assert.Equal("Not provided", v.TextContent.Trim()));
        AssertSummaryListValue(doc, "Email", v => Assert.Equal("Not provided", v.TextContent.Trim()));
        AssertSummaryListValue(doc, "Mobile number", v => Assert.Equal("Not provided", v.TextContent.Trim()));
        AssertSummaryListValue(doc, "National Insurance number", v => Assert.Equal("Not provided", v.TextContent.Trim()));
    }

    [Fact]
    public async Task Get_ShowsChangeReasonAndEvidenceFile_AsExpected()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();
        var editEditDetailsState = new EditDetailsStateBuilder()
            .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
            .WithChangeReasonChoice(EditDetailsChangeReasonOption.AnotherReason, _changeReasonDetails)
            .WithUploadEvidenceChoice(true, evidenceFileId, "evidence.pdf", "1.2 MB")
            .Build();

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editEditDetailsState);

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        AssertSummaryListValue(doc, "Reason for changing record", v => Assert.Equal("Another reason", v.TextContent.Trim()));
        AssertSummaryListValue(doc, "Reason details", v => Assert.Equal(_changeReasonDetails, v.TextContent.Trim()));
        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";
        AssertSummaryListValue(doc, "Do you have evidence to upload", v =>
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
        var editEditDetailsState = new EditDetailsStateBuilder()
            .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
            .WithChangeReasonChoice(EditDetailsChangeReasonOption.IncompleteDetails)
            .WithUploadEvidenceChoice(false)
            .Build();

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editEditDetailsState);

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        AssertSummaryListValue(doc, "Reason for changing record", v => Assert.Equal("Data loss or incomplete information", v.TextContent.Trim()));
        AssertSummaryListValue(doc, "Reason details", v => Assert.Equal("Not provided", v.TextContent.Trim()));
        AssertSummaryListValue(doc, "Do you have evidence to upload", v => Assert.Equal("Not provided", v.TextContent.Trim()));
    }

    [Fact]
    public async Task Post_Confirm_UpdatesPersonEditDetailsCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var evidenceFileId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState("Alfred", "The", "Great", DateOnly.Parse("1 Feb 1980"), "test@test.com", "07891 234567", "AB 12 34 56 C")
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
            Assert.Equal("Alfred", updatedPersonRecord.FirstName);
            Assert.Equal("The", updatedPersonRecord.MiddleName);
            Assert.Equal("Great", updatedPersonRecord.LastName);
            Assert.Equal(DateOnly.Parse("1 Feb 1980"), updatedPersonRecord.DateOfBirth);
            Assert.Equal("test@test.com", updatedPersonRecord.EmailAddress);
            Assert.Equal("447891234567", updatedPersonRecord.MobileNumber);
            Assert.Equal("AB123456C", updatedPersonRecord.NationalInsuranceNumber);
        });

        var RaisedBy = GetCurrentUserId();

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<PersonDetailsUpdatedEvent>(e);

            Assert.Equal(Clock.UtcNow, actualEvent.CreatedUtc);
            Assert.Equal(person.PersonId, actualEvent.PersonId);
            Assert.Equal("Alfred", actualEvent.Details.FirstName);
            Assert.Equal("The", actualEvent.Details.MiddleName);
            Assert.Equal("Great", actualEvent.Details.LastName);
            Assert.Equal(DateOnly.Parse("1 Feb 1980"), actualEvent.Details.DateOfBirth);
            Assert.Equal("test@test.com", actualEvent.Details.EmailAddress);
            Assert.Equal("447891234567", actualEvent.Details.MobileNumber);
            Assert.Equal("AB123456C", actualEvent.Details.NationalInsuranceNumber);
            Assert.Equal("Another reason", actualEvent.ChangeReason);
            Assert.Equal(_changeReasonDetails, actualEvent.ChangeReasonDetail);
            Assert.Equal(evidenceFileId, actualEvent.EvidenceFile!.FileId);
            Assert.Equal("evidencefile.png", actualEvent.EvidenceFile.Name);
            var expectedChanges =
                PersonDetailsUpdatedEventChanges.FirstName |
                PersonDetailsUpdatedEventChanges.MiddleName |
                PersonDetailsUpdatedEventChanges.LastName |
                PersonDetailsUpdatedEventChanges.DateOfBirth |
                PersonDetailsUpdatedEventChanges.EmailAddress |
                PersonDetailsUpdatedEventChanges.MobileNumber |
                PersonDetailsUpdatedEventChanges.NationalInsuranceNumber;
            Assert.Equal(expectedChanges, actualEvent.Changes);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    private string GetRequestPath(TestData.CreatePersonResult person) =>
        $"/persons/{person.PersonId}/edit-details/check-answers";

    private string GetRequestPath(TestData.CreatePersonResult person, JourneyInstance<EditDetailsState> journeyInstance) =>
        $"/persons/{person.PersonId}/edit-details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

    private string GetRequestPath(TestData.CreatePersonResult person, JourneyInstance<EditDetailsState> journeyInstance, bool fromCheckAnswers) =>
        $"/persons/{person.PersonId}/edit-details/check-answers?fromCheckAnswers={fromCheckAnswers}&{journeyInstance.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<EditDetailsState>> CreateJourneyInstanceAsync(Guid personId, EditDetailsState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditDetails,
            state ?? new EditDetailsState(),
            new KeyValuePair<string, object>("personId", personId));
}
