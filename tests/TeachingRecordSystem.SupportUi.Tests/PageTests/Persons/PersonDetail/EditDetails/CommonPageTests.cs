using System.Globalization;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;
using PersonDetailsUpdatedEventChanges = TeachingRecordSystem.Core.Events.Legacy.PersonDetailsUpdatedEventChanges;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public class CommonPageTests(HostFixture hostFixture) : EditDetailsTestBase(hostFixture)
{
    public static TheoryData<string, string?, HttpMethod> UserDoesNotHavePermission_ReturnsForbiddenData =>
        new MatrixTheoryData<string, string?, HttpMethod>(
            [
                "/edit-details",
                "/edit-details/other-details-change-reason",
                "/edit-details/name-change-reason",
                "/edit-details/check-answers"
            ],
            [UserRoles.Viewer, null],
            TestHttpMethods.GetAndPost.SplitTestMethods().ToArray());

    [Theory]
    [MemberData(nameof(UserDoesNotHavePermission_ReturnsForbiddenData))]
    public async Task UserDoesNotHavePermission_ReturnsForbidden(string page, string? role, HttpMethod httpMethod)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
            }));

        var request = new HttpRequestMessage(httpMethod,
            $"/persons/{person.PersonId}{page}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [PathAndHttpMethodsData(
        [
            "/edit-details",
            "/edit-details/other-details-change-reason",
            "/edit-details/name-change-reason",
            "/edit-details/check-answers"
        ],
        TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(string page, HttpMethod httpMethod)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
            }));

        var request = new HttpRequestMessage(httpMethod,
            $"/persons/{person.PersonId}{page}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Theory]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange, false, false, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange, false, true, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, true, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.OtherThanNameChange, true, false, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, true, false, "/edit-details/other-details-change-reason")]
    public async Task Get_CheckAnswersWithUnansweredReason_RedirectsToReasonPage(string attemptedPage, PersonDetailsUpdatedEventChanges changes, bool hasNameChangeReason, bool hasOtherDetailsChangeReason, string expectedPage)
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

        var state = CreateState(person, s =>
                    {
                        s.FirstName = firstName;
                        s.MiddleName = middleName;
                        s.LastName = lastName;
                        s.DateOfBirth = dateOfBirth;
                        s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(emailAddress);
                        s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(nationalInsuranceNumber);
                        s.Gender = gender;
                    });

        if (hasNameChangeReason)
        {
            state.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
            state.NameChangeEvidence = new() { UploadEvidence = false };
        }

        if (hasOtherDetailsChangeReason)
        {
            state.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
            state.OtherDetailsChangeEvidence = new() { UploadEvidence = false };
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);
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

        var state = CreateState(person, s =>
                    {
                        s.FirstName = firstName;
                        s.MiddleName = middleName;
                        s.LastName = lastName;
                        s.DateOfBirth = dateOfBirth;
                        s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(emailAddress);
                        s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(nationalInsuranceNumber);
                    });

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            state.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
            state.NameChangeEvidence = new() { UploadEvidence = false };
        }

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            state.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
            state.OtherDetailsChangeEvidence = new() { UploadEvidence = false };
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);
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
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange, false, false, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange, false, true, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, true, "/edit-details/name-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.OtherThanNameChange, true, false, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/check-answers", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, true, false, "/edit-details/other-details-change-reason")]
    public async Task Post_CheckAnswersWithUnansweredReason_RedirectsToReasonPage(string attemptedPage, PersonDetailsUpdatedEventChanges changes, bool hasNameChangeReason, bool hasOtherDetailsChangeReason, string expectedPage)
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

        var state = CreateState(person, s =>
                    {
                        s.FirstName = firstName;
                        s.MiddleName = middleName;
                        s.LastName = lastName;
                        s.DateOfBirth = dateOfBirth;
                        s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(emailAddress);
                        s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(nationalInsuranceNumber);
                        s.Gender = gender;
                    });

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
            state.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
            state.NameChangeEvidence = new() { UploadEvidence = false };

            content = content
                .WithReason(PersonNameChangeReason.CorrectingAnError)
                .WithUploadEvidence(false);
        }

        if (hasOtherDetailsChangeReason)
        {
            state.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
            state.OtherDetailsChangeEvidence = new() { UploadEvidence = false };

            content = content
                .WithReason(PersonDetailsChangeReason.IncompleteDetails)
                .WithUploadEvidence(false);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);
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

    [Theory]
    // Edit details (name changes only): redirects to change name reason page
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.FirstName, "/edit-details/name-change-reason")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.MiddleName, "/edit-details/name-change-reason")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.LastName, "/edit-details/name-change-reason")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/name-change-reason")]
    // Edit details (other details changes only): redirects to change reason page
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.DateOfBirth, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.EmailAddress, "/edit-details/other-details-change-reason")]
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
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.Gender, "/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/name-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    // Change reason (other details changes only): redirects to check answers page
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.DateOfBirth, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.EmailAddress, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.Gender, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/check-answers")]
    // Change reason (name and other details changes): redirects to check answers page
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.DateOfBirth, "/edit-details/check-answers")]
    [InlineData("/edit-details/other-details-change-reason", PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.EmailAddress, "/edit-details/check-answers")]
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

        var state = CreateState(person, s =>
                    {
                        s.FirstName = firstName;
                        s.MiddleName = middleName;
                        s.LastName = lastName;
                        s.DateOfBirth = dateOfBirth;
                        s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(emailAddress);
                        s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(nationalInsuranceNumber);
                        s.Gender = gender;
                    });

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            state.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
            state.NameChangeEvidence = new() { UploadEvidence = false };
        }

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            state.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
            state.OtherDetailsChangeEvidence = new() { UploadEvidence = false };
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
                .WithReason(PersonNameChangeReason.CorrectingAnError)
                .WithUploadEvidence(false);
        }

        if (fromPage.Contains("other-details"))
        {
            content = content
                .WithReason(PersonDetailsChangeReason.IncompleteDetails)
                .WithUploadEvidence(false);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);
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

    [Theory]
    [InlineData("/edit-details")]
    [InlineData("/edit-details/name-change-reason")]
    [InlineData("/edit-details/other-details-change-reason")]
    [InlineData("/edit-details/check-answers")]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetailsPage(string fromPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
                s.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false
                };
                s.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
                s.OtherDetailsChangeEvidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var pageUrl = $"/persons/{person.PersonId}{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;
        Assert.NotNull(cancelButton);
        Assert.Equal("Cancel", cancelButton.Name);

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, pageUrl)
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        var location = redirectResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}", location);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
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
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
                s.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false
                };
                s.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
                s.OtherDetailsChangeEvidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = evidenceFileId,
                        FileName = "evidence.jpg",
                        FileSizeDescription = "1.2 KB"
                    }
                };
            }));

        var pageUrl = $"/persons/{person.PersonId}{page}?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, pageUrl)
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };
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
    public async Task Get_WithReturnUrlToCheckAnswersPage_BacklinkLinksToCheckAnswersPage(string fromPage, PersonDetailsUpdatedEventChanges changes, string expectedBackPage)
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

        var state = CreateState(person, s =>
                    {
                        s.FirstName = firstName;
                        s.MiddleName = middleName;
                        s.LastName = lastName;
                        s.DateOfBirth = dateOfBirth;
                        s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(emailAddress);
                        s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(nationalInsuranceNumber);
                        s.Gender = gender;
                    });

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            state.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
            state.NameChangeEvidence = new() { UploadEvidence = false };
        }

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            state.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
            state.OtherDetailsChangeEvidence = new() { UploadEvidence = false };
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);
        var checkAnswersUrl = $"/persons/{person.PersonId}/edit-details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}{fromPage}?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NameChange, PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason?returnUrl={0}&")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/name-change-reason?returnUrl={0}&")]
    // Adds name/other details change: redirects to appropriate reason page
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.NameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason?returnUrl={0}&")]
    [InlineData("/edit-details", PersonDetailsUpdatedEventChanges.OtherThanNameChange, PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/name-change-reason?returnUrl={0}&")]
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
    public async Task Post_WithReturnUrlToCheckAnswersPage_AndMoreChangesMade_RedirectsToExpectedPage(string fromPage, PersonDetailsUpdatedEventChanges originalChanges, PersonDetailsUpdatedEventChanges newChanges, string expectedNextPageUrl)
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

        var state = CreateState(person, s =>
                    {
                        s.FirstName = originalFirstName;
                        s.MiddleName = originalMiddleName;
                        s.LastName = originalLastName;
                        s.DateOfBirth = originalDateOfBirth;
                        s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(originalEmailAddress);
                        s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(originalNationalInsuranceNumber);
                        s.Gender = originalGender;
                    });

        if (originalChanges.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            state.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
            state.NameChangeEvidence = new() { UploadEvidence = false };
        }

        if (originalChanges.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            state.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
            state.OtherDetailsChangeEvidence = new() { UploadEvidence = false };
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
                .WithReason(PersonNameChangeReason.CorrectingAnError)
                .WithUploadEvidence(false);
        }

        if (fromPage.Contains("other-details"))
        {
            content = content
                .WithReason(PersonDetailsChangeReason.IncompleteDetails)
                .WithUploadEvidence(false);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);
        var checkAnswersUrl = $"/persons/{person.PersonId}/edit-details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}{fromPage}?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = content.BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}{string.Format(CultureInfo.InvariantCulture, expectedNextPageUrl, Uri.EscapeDataString(checkAnswersUrl))}{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

}
