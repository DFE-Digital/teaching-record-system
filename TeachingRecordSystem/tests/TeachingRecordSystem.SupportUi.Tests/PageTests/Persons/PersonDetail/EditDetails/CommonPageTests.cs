using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public class CommonPageTests : TestBase
{
    public CommonPageTests(HostFixture hostFixture) : base(hostFixture)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
        FileServiceMock.Invocations.Clear();
    }

    public override void Dispose()
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);
        base.Dispose();
    }

    [Theory]
    [InlineData("/edit-details")]
    [InlineData("/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers")]
    public async Task Get_FeatureFlagDisabled_ReturnsNotFound(string page)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);

        // Arrange
        var person = await TestData.CreatePersonAsync();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}{page}");
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);

        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
    }

    [Theory]
    [MemberData(nameof(GetPagesForUserWithoutPersonDataEditPermissionData))]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden(string page, string? role)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"/persons/{person.PersonId}/{page}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.None, false, false, "/edit-details")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.None, false, false, "/edit-details")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange, false, false, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange, false, true, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, true, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.None, false, false, "/edit-details")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange, false, false, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange, false, true, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, true, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.OtherThanNameChange, true, false, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, true, false, "/edit-details/other-details-change-reason")]
    public async Task Get_InvalidState_RedirectsToAppropriatePage(string attemptedPage, PersonDetailsUpdatedEventChanges changes, bool hasNameChangeReason, bool hasOtherDetailsChangeReason, string expectedPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmail("some@email-address.com")
            .WithMobileNumber("07891234567")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Core.Dqt.Models.Contact_GenderCode.Other));

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.Email;
        var mobileNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.MobileNumber) ? "07654321987" : person.MobileNumber;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : person.Gender;

        var state = new EditDetailsStateBuilder()
            .WithInitializedState(person)
            .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
            .WithMobileNumber(mobileNumber)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithGender(gender);

        if (hasNameChangeReason)
        {
            state = state
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeUploadEvidenceChoice(false);
        }

        if (hasOtherDetailsChangeReason)
        {
            state = state
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                .WithOtherDetailsChangeUploadEvidenceChoice(false);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state.Build());
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}{attemptedPage}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}{expectedPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Theory]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NameChange, "")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange, "/edit-details")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    public async Task Get_BacklinkContainsExpected(string fromPage, PersonDetailsUpdatedEventChanges changes, string expectedBackPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmail("some@email-address.com")
            .WithMobileNumber("07891234567")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Core.Dqt.Models.Contact_GenderCode.Male));

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.Email;
        var mobileNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.MobileNumber) ? "07654321987" : person.MobileNumber;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : person.Gender;

        var state = new EditDetailsStateBuilder()
            .WithInitializedState(person)
            .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
            .WithMobileNumber(mobileNumber)
            .WithNationalInsuranceNumber(nationalInsuranceNumber);

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            state = state
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeUploadEvidenceChoice(false);
        }

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            state = state
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                .WithOtherDetailsChangeUploadEvidenceChoice(false);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state.Build());
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        Assert.Contains($"/persons/{person.PersonId}{expectedBackPage}", backlink.Href);
    }

    [Theory]
    // Edit details (name changes only): redirects to change name reason page
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.FirstName, "/edit-details/name-change-reason")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.MiddleName, "/edit-details/name-change-reason")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.LastName, "/edit-details/name-change-reason")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/name-change-reason")]
    // Edit details (other details changes only): redirects to change reason page
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.DateOfBirth, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.EmailAddress, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.MobileNumber, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.Gender, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    // Edit details (name and other details changes): redirects to change name reason page
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/name-change-reason")]
    // Name change reason (name changes only): redirects to check answers page
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.FirstName, "/edit-details/check-answers")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.MiddleName, "/edit-details/check-answers")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.LastName, "/edit-details/check-answers")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/check-answers")]
    // Name change reason (name and other details changes): redirects to change reason page
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.FirstName | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.MiddleName | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.LastName | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.DateOfBirth, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.EmailAddress, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.MobileNumber, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.Gender, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    // Change reason (other details changes only): redirects to check answers page
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.DateOfBirth, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.EmailAddress, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.MobileNumber, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.Gender, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    // Change reason (name and other details changes): redirects to check answers page
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.DateOfBirth, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.EmailAddress, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.MobileNumber, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.Gender, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    public async Task Post_RedirectsToExpectedPage(string fromPage, PersonDetailsUpdatedEventChanges changes, string expectedNextPageUrl)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmail("some@email-address.com")
            .WithMobileNumber("07891234567")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Core.Dqt.Models.Contact_GenderCode.Other));

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.Email;
        var mobileNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.MobileNumber) ? "07654321987" : person.MobileNumber;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : person.Gender;

        var state = new EditDetailsStateBuilder()
            .WithInitializedState(person)
            .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
            .WithMobileNumber(mobileNumber)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithGender(gender);

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            state = state
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeUploadEvidenceChoice(false);
        }

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            state = state
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                .WithOtherDetailsChangeUploadEvidenceChoice(false);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state.Build());
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditDetailsPostRequestContentBuilder()
                    .WithFirstName(firstName)
                    .WithMiddleName(middleName)
                    .WithLastName(lastName)
                    .WithDateOfBirth(dateOfBirth)
                    .WithEmailAddress(emailAddress)
                    .WithMobileNumber(mobileNumber)
                    .WithNationalInsuranceNumber(nationalInsuranceNumber)
                    .WithGender(gender)
                    .WithNameChangeReason(EditDetailsNameChangeReasonOption.CorrectingAnError)
                    .WithNameChangeEvidence(false)
                    .WithOtherDetailsChangeReason(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                    .WithOtherDetailsChangeEvidence(false)
                    .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Theory]
    [InlineData("/edit-details")]
    [InlineData("/edit-details/name-change-reason")]
    [InlineData("/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/check-answers")]
    public async Task Post_Cancel_RedirectsToPersonDetailsPage(string fromPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeUploadEvidenceChoice(false)
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                .WithOtherDetailsChangeUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, cancelButton!.FormAction);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        var location = redirectResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}", location);
    }

    [Theory]
    [InlineData("/edit-details")]
    [InlineData("/edit-details/name-change-reason")]
    [InlineData("/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/check-answers")]
    public async Task Post_Cancel_EvidenceFilePreviouslyUploaded_DeletesPreviouslyUploadedFile(string page)
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
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeUploadEvidenceChoice(false)
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                .WithOtherDetailsChangeUploadEvidenceChoice(true, evidenceFileId, "evidence.jpg", "1.2 KB")
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}{page}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, cancelButton!.FormAction);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Theory]
    // Every page goes back to check answers page (even if a new reason page was added to the journey on this visit)
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/check-answers")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/check-answers")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    // Check answers page goes back to the appropriate reason page
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    public async Task Get_WhenLinkedToFromFromCheckAnswersPage_BacklinkContainsExpected(string fromPage, PersonDetailsUpdatedEventChanges changes, string expectedBackPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmail("some@email-address.com")
            .WithMobileNumber("07891234567")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Core.Dqt.Models.Contact_GenderCode.Female));

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.Email;
        var mobileNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.MobileNumber) ? "07654321987" : person.MobileNumber;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Other : person.Gender;

        var state = new EditDetailsStateBuilder()
            .WithInitializedState(person)
            .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
            .WithMobileNumber(mobileNumber)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithGender(gender);

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            state = state
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeUploadEvidenceChoice(false);
        }

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            state = state
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                .WithOtherDetailsChangeUploadEvidenceChoice(false);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state.Build());
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}{fromPage}?FromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}{expectedBackPage}", backlink!.Href);
    }

    [Theory]
    // No new changes: redirects to check answers page
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NameChange, PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/check-answers?")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    // Switches from name change to other details change (& vice versa): redirects to appropriate reason page
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NameChange, PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason?fromCheckAnswers=True&")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/name-change-reason?fromCheckAnswers=True&")]
    // Adds name/other details change: redirects to appropriate reason page
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason?fromCheckAnswers=True&")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/name-change-reason?fromCheckAnswers=True&")]
    // Removes name/other details change: redirects to check answers page
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/check-answers?")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    // Change name reason (whether original or subsequent name change): redirects to check answers page
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange, PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/check-answers?")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    // Change reason (whether original or subsequent other details change): redirects to check answers page
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    public async Task Post_WhenLinkedToFromCheckAnswersPage_AndMoreChangesMade_RedirectsToExpectedPage(string page, PersonDetailsUpdatedEventChanges originalChanges, PersonDetailsUpdatedEventChanges newChanges, string expectedNextPageUrl)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmail("some@email-address.com")
            .WithMobileNumber("07891234567")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Core.Dqt.Models.Contact_GenderCode.Female));

        var originalFirstName = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var originalMiddleName = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var originalLastName = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var originalDateOfBirth = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var originalEmailAddress = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.Email;
        var originalMobileNumber = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.MobileNumber) ? "07654321987" : person.MobileNumber;
        var originalNationalInsuranceNumber = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var originalGender = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Male : person.Gender;

        var newFirstName = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var newMiddleName = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var newLastName = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var newDateOfBirth = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var newEmailAddress = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.Email;
        var newMobileNumber = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.MobileNumber) ? "07654321987" : person.MobileNumber;
        var newNationalInsuranceNumber = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var newGender = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Male : person.Gender;

        var state = new EditDetailsStateBuilder()
            .WithInitializedState(person)
            .WithName(originalFirstName, originalMiddleName, originalLastName)
            .WithDateOfBirth(originalDateOfBirth)
            .WithEmail(originalEmailAddress)
            .WithMobileNumber(originalMobileNumber)
            .WithNationalInsuranceNumber(originalNationalInsuranceNumber)
            .WithGender(originalGender);

        if (originalChanges.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            state = state
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeUploadEvidenceChoice(false);
        }

        if (originalChanges.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            state = state
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                .WithOtherDetailsChangeUploadEvidenceChoice(false);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state.Build());
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}{page}?fromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithFirstName(newFirstName)
                .WithMiddleName(newMiddleName)
                .WithLastName(newLastName)
                .WithDateOfBirth(newDateOfBirth)
                .WithEmailAddress(newEmailAddress)
                .WithMobileNumber(newMobileNumber)
                .WithNationalInsuranceNumber(newNationalInsuranceNumber)
                .WithGender(newGender)
                .WithNameChangeReason(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeEvidence(false)
                .WithOtherDetailsChangeReason(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                .WithOtherDetailsChangeEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}{expectedNextPageUrl}{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    public static TheoryData<string, string?> GetPagesForUserWithoutPersonDataEditPermissionData()
    {
        var pages = new[] { "edit-details", "edit-details/name-change-reason", "edit-details/other-details-change-reason", "edit-details/check-answers" };

        var rolesWithoutWritePermission = new[] { UserRoles.Viewer }
            .Append(null)
            .ToArray();

        var data = new TheoryData<string, string?>();

        foreach (var page in pages)
        {
            foreach (var role in rolesWithoutWritePermission)
            {
                data.Add(page, role);
            }
        }

        return data;
    }

    private Task<JourneyInstance<EditDetailsState>> CreateJourneyInstanceAsync(Guid personId, EditDetailsState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditDetails,
            state ?? new EditDetailsState(),
            new KeyValuePair<string, object>("personId", personId));
}
