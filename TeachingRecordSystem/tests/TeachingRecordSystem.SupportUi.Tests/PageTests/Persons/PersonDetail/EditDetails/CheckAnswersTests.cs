using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

[Collection(nameof(DisableParallelization))]
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
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var caption = doc.GetElementByTestId("edit-details-caption");
        Assert.Equal("Change personal details - Alfred The Great", caption!.TrimmedText());
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
                .WithName("A", "New", "Name")
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeUploadEvidenceChoice(false)
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
            b => Assert.Equal("Confirm changes", b.TrimmedText()),
            b => Assert.Equal("Cancel and return to record", b.TrimmedText()));
    }

    [Fact]
    public async Task Get_ShowsPersonalDetails_AsExpected()
    {
        // Arrange

        var person = await TestData.CreatePersonAsync(p => p.WithTrn());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithEmail("test@test.com")
                .WithMobileNumber("07891 234567")
                .WithNationalInsuranceNumber("AB 12 34 56 C")
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeUploadEvidenceChoice(false)
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                .WithOtherDetailsChangeUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListValue("Full name", v => Assert.Equal("Alfred The Great", v.TrimmedText()));
        doc.AssertSummaryListValue("Date of birth", v => Assert.Equal("1 February 1980", v.TrimmedText()));
        doc.AssertSummaryListValue("Email address", v => Assert.Equal("test@test.com", v.TrimmedText()));
        doc.AssertSummaryListValue("Mobile number", v => Assert.Equal("07891234567", v.TrimmedText()));
        doc.AssertSummaryListValue("National Insurance number", v => Assert.Equal("AB 12 34 56 C", v.TrimmedText()));
    }

    [Fact]
    public async Task Get_ShowsMissingOptionalPersonalDetails_AsNotProvided()
    {
        // Arrange

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", null, "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeUploadEvidenceChoice(false)
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                .WithOtherDetailsChangeUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListValue("Full name", v => Assert.Equal("Alfred Great", v.TrimmedText()));
        doc.AssertSummaryListValue("Email address", v => Assert.Equal("Not provided", v.TrimmedText()));
        doc.AssertSummaryListValue("Mobile number", v => Assert.Equal("Not provided", v.TrimmedText()));
        doc.AssertSummaryListValue("National Insurance number", v => Assert.Equal("Not provided", v.TrimmedText()));
    }

    [Fact]
    public async Task Get_WhenNameFieldChanged_ShowsNameChangeReasonAndEvidenceFile_AsExpected()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", "The", "Great")
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.DeedPollOrOtherLegalProcess)
                .WithNameChangeUploadEvidenceChoice(true, evidenceFileId, "evidence.pdf", "1.2 MB")
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListValue("Reason for name change", v => Assert.Equal("Name has changed by deed poll or another legal process", v.TrimmedText()));
        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";
        doc.AssertSummaryListValues("Evidence uploaded", v =>
        {
            var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(v.QuerySelector("a"));
            Assert.Equal("evidence.pdf (opens in new tab)", link.TrimmedText());
            Assert.Equal(expectedFileUrl, link.Href);
        });

        doc.AssertSummaryListRowDoesNotExist("Personal details change");
        doc.AssertSummaryListRowDoesNotExist("Other personal details change");
        doc.AssertSummaryListRowDoesNotExist("Reason details");
        doc.AssertSummaryListRowDoesNotExist("Evidence");
    }

    [Fact]
    public async Task Get_WhenOtherDetailsFieldChanged_ShowsDetailsChangeReasonAndEvidenceFile_AsExpected()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.AnotherReason, _changeReasonDetails)
                .WithOtherDetailsChangeUploadEvidenceChoice(true, evidenceFileId, "evidence.pdf", "1.2 MB")
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListValue("Reason for personal details change", v => Assert.Equal("Another reason", v.TrimmedText()));
        doc.AssertSummaryListValue("Reason details", v => Assert.Equal(_changeReasonDetails, v.TrimmedText()));
        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";
        doc.AssertSummaryListValue("Evidence uploaded", v =>
        {
            var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(v.QuerySelector("a"));
            Assert.Equal("evidence.pdf (opens in new tab)", link.TrimmedText());
            Assert.Equal(expectedFileUrl, link.Href);
        });

        doc.AssertSummaryListRowDoesNotExist("Reason for name change");
        doc.AssertSummaryListRowDoesNotExist("Other personal details change");
        doc.AssertSummaryListRowDoesNotExist("Evidence");
    }

    [Fact]
    public async Task Get_WhenNameAndOtherFieldsChanged_ShowsNameChangeReasonAndOtherChangeReasonAndEvidenceFiles_AsExpected()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.DeedPollOrOtherLegalProcess)
                .WithNameChangeUploadEvidenceChoice(true, evidenceFileId, "name-evidence.pdf", "2.4 MB")
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.AnotherReason, _changeReasonDetails)
                .WithOtherDetailsChangeUploadEvidenceChoice(true, evidenceFileId, "other-evidence.pdf", "1.2 MB")
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListValue("Reason for name change", v => Assert.Equal("Name has changed by deed poll or another legal process", v.TrimmedText()));
        doc.AssertSummaryListValue("Other personal details change", v => Assert.Equal("Another reason", v.TrimmedText()));
        doc.AssertSummaryListValue("Reason details", v => Assert.Equal(_changeReasonDetails, v.TrimmedText()));
        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";
        doc.AssertSummaryListValues("Evidence", v =>
        {
            var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(v.QuerySelector("a"));
            Assert.Equal("name-evidence.pdf (opens in new tab)", link.TrimmedText());
            Assert.Equal(expectedFileUrl, link.Href);
        }, v =>
        {
            var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(v.QuerySelector("a"));
            Assert.Equal("other-evidence.pdf (opens in new tab)", link.TrimmedText());
            Assert.Equal(expectedFileUrl, link.Href);
        });

        doc.AssertSummaryListRowDoesNotExist("Reason for personal details change");
    }

    [Fact]
    public async Task Get_WhenNameFieldChanged_ShowsMissingAdditionalDetailAndEvidenceFile_AsNotProvided()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", "The", "Great")
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.DeedPollOrOtherLegalProcess)
                .WithNameChangeUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListValue("Reason for name change", v => Assert.Equal("Name has changed by deed poll or another legal process", v.TrimmedText()));
        doc.AssertSummaryListValues("Evidence uploaded", v => Assert.Equal("Not provided", v.TrimmedText()));

        doc.AssertSummaryListRowDoesNotExist("Other personal details change");
        doc.AssertSummaryListRowDoesNotExist("Reason for personal details change");
        doc.AssertSummaryListRowDoesNotExist("Reason details");
        doc.AssertSummaryListRowDoesNotExist("Evidence");
    }

    [Fact]
    public async Task Get_WhenOtherDetailsFieldChanged_ShowsMissingAdditionalDetailAndEvidenceFile_AsNotProvided()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                .WithOtherDetailsChangeUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListValue("Reason for personal details change", v => Assert.Equal("Data loss or incomplete information", v.TrimmedText()));
        doc.AssertSummaryListValue("Reason details", v => Assert.Equal("Not provided", v.TrimmedText()));
        doc.AssertSummaryListValues("Evidence uploaded", v => Assert.Equal("Not provided", v.TrimmedText()));

        doc.AssertSummaryListRowDoesNotExist("Other personal details change");
        doc.AssertSummaryListRowDoesNotExist("Reason for name change");
        doc.AssertSummaryListRowDoesNotExist("Evidence");
    }

    [Theory]
    [InlineData(PersonDetailsUpdatedEventChanges.FirstName)]
    [InlineData(PersonDetailsUpdatedEventChanges.MiddleName)]
    [InlineData(PersonDetailsUpdatedEventChanges.LastName)]
    [InlineData(PersonDetailsUpdatedEventChanges.DateOfBirth)]
    [InlineData(PersonDetailsUpdatedEventChanges.EmailAddress)]
    [InlineData(PersonDetailsUpdatedEventChanges.MobileNumber)]
    [InlineData(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber)]
    [InlineData(PersonDetailsUpdatedEventChanges.FirstName | PersonDetailsUpdatedEventChanges.MiddleName | PersonDetailsUpdatedEventChanges.LastName | PersonDetailsUpdatedEventChanges.DateOfBirth | PersonDetailsUpdatedEventChanges.EmailAddress | PersonDetailsUpdatedEventChanges.MobileNumber | PersonDetailsUpdatedEventChanges.NationalInsuranceNumber)]
    public async Task Post_Confirm_UpdatesPersonEditDetailsCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(PersonDetailsUpdatedEventChanges changes)
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

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Jim" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "A" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Person" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("3 July 1990") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email.com" : person.Email;
        var mobileNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.MobileNumber) ? "447654321987" : person.MobileNumber;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "JK987654D" : person.NationalInsuranceNumber;

        var nameEvidenceFileId = Guid.NewGuid();
        var otherEvidenceFileId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName(firstName, middleName, lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithEmail(emailAddress)
                .WithMobileNumber(mobileNumber)
                .WithNationalInsuranceNumber(nationalInsuranceNumber)
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeUploadEvidenceChoice(false, nameEvidenceFileId, "name-evidence.pdf", "2.4 MB")
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.AnotherReason, _changeReasonDetails)
                .WithOtherDetailsChangeUploadEvidenceChoice(true, otherEvidenceFileId, "other-evidence.png")
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
            Assert.Equal(Clock.UtcNow, updatedPersonRecord.UpdatedOn);
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
            Assert.Equal("Correcting an error", actualEvent.NameChangeReason);
            Assert.Equal(nameEvidenceFileId, actualEvent.NameChangeEvidenceFile!.FileId);
            Assert.Equal("name-evidence.pdf", actualEvent.NameChangeEvidenceFile.Name);
            Assert.Equal("Another reason", actualEvent.DetailsChangeReason);
            Assert.Equal(_changeReasonDetails, actualEvent.DetailsChangeReasonDetail);
            Assert.Equal(otherEvidenceFileId, actualEvent.DetailsChangeEvidenceFile!.FileId);
            Assert.Equal("other-evidence.png", actualEvent.DetailsChangeEvidenceFile.Name);
            Assert.Equal(changes, actualEvent.Changes);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_Confirm_WhenAnyNameFieldChanged_AndNameChangeReasonIsCorrectingAnError_DoesNotUpdatePersonPreviousNames()
    {
        // Arrange
        var ethelredDate = DateTime.Parse("1 Jan 1990").ToUniversalTime();
        var conanDate = DateTime.Parse("1 Jan 1991").ToUniversalTime();
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithPreviousNames([("Ethelred", "The", "Unready", ethelredDate), ("Conan", "The", "Barbarian", conanDate)])
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfrede", "Thee", "Greate")
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeUploadEvidenceChoice(false)
                .Build());

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
            var updatedPersonRecord = await dbContext.Persons
                .Include(p => p.PreviousNames)
                .SingleAsync(p => p.PersonId == person.PersonId);

            Assert.Equal(Clock.UtcNow, updatedPersonRecord.UpdatedOn);
            Assert.Equal("Alfrede", updatedPersonRecord.FirstName);
            Assert.Equal("Thee", updatedPersonRecord.MiddleName);
            Assert.Equal("Greate", updatedPersonRecord.LastName);

            Assert.Collection(updatedPersonRecord.PreviousNames!.OrderByDescending(pn => pn.CreatedOn),
                pn => Assert.Equal(("Conan", "The", "Barbarian", conanDate, conanDate), (pn.FirstName, pn.MiddleName, pn.LastName, pn.CreatedOn, pn.UpdatedOn)),
                pn => Assert.Equal(("Ethelred", "The", "Unready", ethelredDate, ethelredDate), (pn.FirstName, pn.MiddleName, pn.LastName, pn.CreatedOn, pn.UpdatedOn)));
        });
    }

    [Theory]
    [InlineData(EditDetailsNameChangeReasonOption.DeedPollOrOtherLegalProcess)]
    [InlineData(EditDetailsNameChangeReasonOption.MarriageOrCivilPartnership)]
    public async Task Post_Confirm_WhenAnyNameFieldChanged_AndNameChangeReasonIsFormalNameChange_UpdatesPersonPreviousNames(EditDetailsNameChangeReasonOption reason)
    {
        // Arrange
        var ethelredDate = DateTime.Parse("1 Jan 1990").ToUniversalTime();
        var conanDate = DateTime.Parse("1 Jan 1991").ToUniversalTime();
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithPreviousNames([("Ethelred", "The", "Unready", ethelredDate), ("Conan", "The", "Barbarian", conanDate)])
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Megan", "Thee", "Stallion")
                .WithNameChangeReasonChoice(reason)
                .WithNameChangeUploadEvidenceChoice(false)
                .Build());

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
            var updatedPersonRecord = await dbContext.Persons
                .Include(p => p.PreviousNames)
                .SingleAsync(p => p.PersonId == person.PersonId);

            Assert.Equal(Clock.UtcNow, updatedPersonRecord.UpdatedOn);
            Assert.Equal("Megan", updatedPersonRecord.FirstName);
            Assert.Equal("Thee", updatedPersonRecord.MiddleName);
            Assert.Equal("Stallion", updatedPersonRecord.LastName);

            Assert.Collection(updatedPersonRecord.PreviousNames!.OrderByDescending(pn => pn.CreatedOn),
                pn => Assert.Equal(("Alfred", "The", "Great", Clock.UtcNow, Clock.UtcNow), (pn.FirstName, pn.MiddleName, pn.LastName, pn.CreatedOn, pn.UpdatedOn)),
                pn => Assert.Equal(("Conan", "The", "Barbarian", conanDate, conanDate), (pn.FirstName, pn.MiddleName, pn.LastName, pn.CreatedOn, pn.UpdatedOn)),
                pn => Assert.Equal(("Ethelred", "The", "Unready", ethelredDate, ethelredDate), (pn.FirstName, pn.MiddleName, pn.LastName, pn.CreatedOn, pn.UpdatedOn)));
        });
    }

    private string GetRequestPath(TestData.CreatePersonResult person, JourneyInstance<EditDetailsState> journeyInstance) =>
        $"/persons/{person.PersonId}/edit-details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<EditDetailsState>> CreateJourneyInstanceAsync(Guid personId, EditDetailsState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditDetails,
            state ?? new EditDetailsState(),
            new KeyValuePair<string, object>("personId", personId));
}
