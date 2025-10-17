using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public class CommonPageTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    [MatrixDataSource]
    public async Task UserDoesNotHavePermission_ReturnsForbidden(
        [Matrix(
            "/edit-details",
            "/edit-details/other-details-change-reason",
            "/edit-details/name-change-reason",
            "/edit-details/check-answers")] string page,
        [Matrix(UserRoles.Viewer, null)] string? role,
        [MatrixHttpMethods(TestHttpMethods.GetAndPost)] HttpMethod httpMethod)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .Build());

        var request = new HttpRequestMessage(httpMethod,
            $"/persons/{person.PersonId}{page}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    [MatrixDataSource]
    public async Task PersonIsDeactivated_ReturnsBadRequest(
        [Matrix(
            "/edit-details",
            "/edit-details/other-details-change-reason",
            "/edit-details/name-change-reason",
            "/edit-details/check-answers")] string page,
        [MatrixHttpMethods(TestHttpMethods.GetAndPost)] HttpMethod httpMethod)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .Build());

        var request = new HttpRequestMessage(httpMethod,
            $"/persons/{person.PersonId}{page}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.None, false, false, "/edit-details")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.None, false, false, "/edit-details")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange, false, false, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange, false, true, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, true, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.None, false, false, "/edit-details")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange, false, false, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange, false, true, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, true, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.OtherThanNameChange, true, false, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, true, false, "/edit-details/other-details-change-reason")]
    public async Task Get_InvalidState_RedirectsToAppropriatePage(string attemptedPage, PersonDetailsUpdatedEventChanges changes, bool hasNameChangeReason, bool hasOtherDetailsChangeReason, string expectedPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("some@email-address.com")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Gender.Other));

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.EmailAddress;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : person.Gender;

        var state = new EditDetailsStateBuilder()
            .WithInitializedState(person)
            .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
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

    [Test]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.NameChange, "")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange, "/edit-details")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    public async Task Get_BacklinkContainsExpected(string fromPage, PersonDetailsUpdatedEventChanges changes, string expectedBackPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("some@email-address.com")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Gender.Male));

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.EmailAddress;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : person.Gender;

        var state = new EditDetailsStateBuilder()
            .WithInitializedState(person)
            .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
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

    [Test]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.None, false, false, "/edit-details")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.None, false, false, "/edit-details")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange, false, false, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange, false, true, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, true, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.None, false, false, "/edit-details")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange, false, false, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange, false, true, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, true, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.OtherThanNameChange, true, false, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, true, false, "/edit-details/other-details-change-reason")]
    public async Task Post_InvalidState_RedirectsToAppropriatePage(string attemptedPage, PersonDetailsUpdatedEventChanges changes, bool hasNameChangeReason, bool hasOtherDetailsChangeReason, string expectedPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("some@email-address.com")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Gender.Other));

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.EmailAddress;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : person.Gender;

        var state = new EditDetailsStateBuilder()
            .WithInitializedState(person)
            .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithGender(gender);

        var content = new EditDetailsPostRequestContentBuilder()
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmailAddress(emailAddress)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithGender(gender);

        if (hasNameChangeReason)
        {
            state = state
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeUploadEvidenceChoice(false);

            content = content
                .WithReason(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithUploadEvidence(false);
        }

        if (hasOtherDetailsChangeReason)
        {
            state = state
                .WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                .WithOtherDetailsChangeUploadEvidenceChoice(false);

            content = content
                .WithReason(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                .WithUploadEvidence(false);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state.Build());
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}{attemptedPage}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = content.BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}{expectedPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Test]
    // Edit details (name changes only): redirects to change name reason page
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.FirstName, "/edit-details/name-change-reason")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.MiddleName, "/edit-details/name-change-reason")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.LastName, "/edit-details/name-change-reason")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/name-change-reason")]
    // Edit details (other details changes only): redirects to change reason page
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.DateOfBirth, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.EmailAddress, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.Gender, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    // Edit details (name and other details changes): redirects to change name reason page
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/name-change-reason")]
    // Name change reason (name changes only): redirects to check answers page
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.FirstName, "/edit-details/check-answers")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.MiddleName, "/edit-details/check-answers")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.LastName, "/edit-details/check-answers")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/check-answers")]
    // Name change reason (name and other details changes): redirects to change reason page
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.FirstName | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.MiddleName | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.LastName | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.DateOfBirth, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.EmailAddress, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.Gender, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    // Change reason (other details changes only): redirects to check answers page
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.DateOfBirth, "/edit-details/check-answers")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.EmailAddress, "/edit-details/check-answers")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, "/edit-details/check-answers")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.Gender, "/edit-details/check-answers")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    // Change reason (name and other details changes): redirects to check answers page
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.DateOfBirth, "/edit-details/check-answers")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.EmailAddress, "/edit-details/check-answers")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, "/edit-details/check-answers")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.Gender, "/edit-details/check-answers")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    public async Task Post_RedirectsToExpectedPage(string fromPage, PersonDetailsUpdatedEventChanges changes, string expectedNextPageUrl)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("some@email-address.com")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Gender.Other));

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.EmailAddress;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : person.Gender;

        var state = new EditDetailsStateBuilder()
            .WithInitializedState(person)
            .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
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

        var content = new EditDetailsPostRequestContentBuilder()
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmailAddress(emailAddress)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithGender(gender);

        if (fromPage.Contains("name"))
        {
            content = content
                .WithReason(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithUploadEvidence(false);
        }

        if (fromPage.Contains("other-details"))
        {
            content = content
                .WithReason(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                .WithUploadEvidence(false);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state.Build());
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = content.BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Test]
    [Arguments("/edit-details")]
    [Arguments("/edit-details/name-change-reason")]
    [Arguments("/edit-details/other-details-change-reason")]
    [Arguments("/edit-details/check-answers")]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetailsPage(string fromPage)
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

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Test]
    [Arguments("/edit-details")]
    [Arguments("/edit-details/name-change-reason")]
    [Arguments("/edit-details/other-details-change-reason")]
    [Arguments("/edit-details/check-answers")]
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

    [Test]
    // Every page goes back to check answers page (even if a new reason page was added to the journey on this visit)
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/check-answers")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/check-answers")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    // Check answers page goes back to the appropriate reason page
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/name-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    [Arguments("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    public async Task Get_WhenLinkedToFromFromCheckAnswersPage_BacklinkContainsExpected(string fromPage, PersonDetailsUpdatedEventChanges changes, string expectedBackPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("some@email-address.com")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Gender.Female));

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.EmailAddress;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Other : person.Gender;

        var state = new EditDetailsStateBuilder()
            .WithInitializedState(person)
            .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
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

    [Test]
    // No new changes: redirects to check answers page
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.NameChange, PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/check-answers?")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    // Switches from name change to other details change (& vice versa): redirects to appropriate reason page
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.NameChange, PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason?fromCheckAnswers=True&")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/name-change-reason?fromCheckAnswers=True&")]
    // Adds name/other details change: redirects to appropriate reason page
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.NameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason?fromCheckAnswers=True&")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/name-change-reason?fromCheckAnswers=True&")]
    // Removes name/other details change: redirects to check answers page
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/check-answers?")]
    [Arguments("/edit-details", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    // Change name reason (whether original or subsequent name change): redirects to check answers page
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange, PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/check-answers?")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    [Arguments("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    // Change reason (whether original or subsequent other details change): redirects to check answers page
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    [Arguments("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers?")]
    public async Task Post_WhenLinkedToFromCheckAnswersPage_AndMoreChangesMade_RedirectsToExpectedPage(string fromPage, PersonDetailsUpdatedEventChanges originalChanges, PersonDetailsUpdatedEventChanges newChanges, string expectedNextPageUrl)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("some@email-address.com")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Gender.Female));

        var originalFirstName = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var originalMiddleName = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var originalLastName = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var originalDateOfBirth = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var originalEmailAddress = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.EmailAddress;
        var originalNationalInsuranceNumber = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var originalGender = originalChanges.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Male : person.Gender;

        var newFirstName = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var newMiddleName = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var newLastName = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var newDateOfBirth = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var newEmailAddress = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.EmailAddress;
        var newNationalInsuranceNumber = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var newGender = newChanges.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Male : person.Gender;

        var state = new EditDetailsStateBuilder()
            .WithInitializedState(person)
            .WithName(originalFirstName, originalMiddleName, originalLastName)
            .WithDateOfBirth(originalDateOfBirth)
            .WithEmail(originalEmailAddress)
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

        var content = new EditDetailsPostRequestContentBuilder()
            .WithFirstName(newFirstName)
            .WithMiddleName(newMiddleName)
            .WithLastName(newLastName)
            .WithDateOfBirth(newDateOfBirth)
            .WithEmailAddress(newEmailAddress)
            .WithNationalInsuranceNumber(newNationalInsuranceNumber)
            .WithGender(newGender);

        if (fromPage.Contains("name"))
        {
            content = content
                .WithReason(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithUploadEvidence(false);
        }

        if (fromPage.Contains("other-details"))
        {
            content = content
                .WithReason(EditDetailsOtherDetailsChangeReasonOption.IncompleteDetails)
                .WithUploadEvidence(false);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state.Build());
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}{fromPage}?fromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = content.BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}{expectedNextPageUrl}{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    private Task<JourneyInstance<EditDetailsState>> CreateJourneyInstanceAsync(Guid personId, EditDetailsState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditDetails,
            state ?? new EditDetailsState(),
            new KeyValuePair<string, object>("personId", personId));
}
