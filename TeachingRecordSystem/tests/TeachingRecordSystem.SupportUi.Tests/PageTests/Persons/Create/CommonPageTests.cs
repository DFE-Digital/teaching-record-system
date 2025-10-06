using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.Create;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Create;

public class CommonPageTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    [MethodDataSource(nameof(GetPagesForUserWithoutPersonDataEditPermissionData))]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden(string page, string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"/persons/create/{page}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    [Arguments("create-reason", false, false, "personal-details")]
    [Arguments("check-answers", false, false, "personal-details")]
    [Arguments("check-answers", true, false, "create-reason")]
    public async Task Get_InvalidState_RedirectsToAppropriatePage(string attemptedPage, bool hasPersonalDetails, bool hasCreateReason, string expectedPage)
    {
        // Arrange
        var firstName = "Alfred";
        var middleName = "The";
        var lastName = "Great";
        var dateOfBirth = DateOnly.Parse("1 Feb 1980");
        var emailAddress = "some@email-address.com";
        var nationalInsuranceNumber = "AB123456D";

        var state = new CreateStateBuilder()
            .WithInitializedState();

        if (hasPersonalDetails)
        {
            state = state
                .WithName(firstName, middleName, lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithEmail(emailAddress)
                .WithNationalInsuranceNumber(nationalInsuranceNumber);
        }

        if (hasCreateReason)
        {
            state = state
                .WithCreateReasonChoice(CreateReasonOption.MandatoryQualification)
                .WithUploadEvidenceChoice(false);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(state.Build());
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/create/{attemptedPage}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/create/{expectedPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Test]
    [Arguments("/create-reason", "/personal-details")]
    [Arguments("/check-answers", "/create-reason")]
    public async Task Get_BacklinkContainsExpected(string fromPage, string expectedBackPage)
    {
        // Arrange
        var firstName = "Alfred";
        var middleName = "The";
        var lastName = "Great";
        var dateOfBirth = DateOnly.Parse("1 Feb 1980");
        var emailAddress = "some@email-address.com";
        var nationalInsuranceNumber = "AB123456D";

        var state = new CreateStateBuilder()
            .WithInitializedState()
            .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithCreateReasonChoice(CreateReasonOption.MandatoryQualification)
            .WithUploadEvidenceChoice(false);

        var journeyInstance = await CreateJourneyInstanceAsync(state.Build());
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/create{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        Assert.Contains($"/persons/create{expectedBackPage}", backlink.Href);
    }

    [Test]
    [Arguments("/personal-details", "/create-reason")]
    [Arguments("/create-reason", "/check-answers")]
    public async Task Post_RedirectsToExpectedPage(string fromPage, string expectedNextPageUrl)
    {
        // Arrange
        var firstName = "Alfred";
        var middleName = "The";
        var lastName = "Great";
        var dateOfBirth = DateOnly.Parse("1 Feb 1980");
        var emailAddress = "some@email-address.com";
        var mobileNumber = "07891234567";
        var nationalInsuranceNumber = "AB123456D";

        var state = new CreateStateBuilder()
            .WithInitializedState()
            .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithCreateReasonChoice(CreateReasonOption.MandatoryQualification)
            .WithUploadEvidenceChoice(false);

        var journeyInstance = await CreateJourneyInstanceAsync(state.Build());
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/create{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName(firstName)
                .WithMiddleName(middleName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithEmailAddress(emailAddress)
                .WithMobileNumber(mobileNumber)
                .WithNationalInsuranceNumber(nationalInsuranceNumber)
                .WithCreateReason(CreateReasonOption.MandatoryQualification)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/create{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Test]
    [Arguments("personal-details")]
    [Arguments("create-reason")]
    [Arguments("check-answers")]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToCreateIndexPage(string page)
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithCreateReasonChoice(CreateReasonOption.MandatoryQualification)
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/create/{page}?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        Assert.Equal($"/persons/create", location);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Test]
    [Arguments("personal-details")]
    [Arguments("create-reason")]
    [Arguments("check-answers")]
    public async Task Post_Cancel_EvidenceFilePreviouslyUploaded_DeletesPreviouslyUploadedFile(string page)
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithCreateReasonChoice(CreateReasonOption.MandatoryQualification)
                .WithUploadEvidenceChoice(true, evidenceFileId, "evidence.jpg", "1.2 KB")
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/create/{page}?{journeyInstance.GetUniqueIdQueryParameter()}");

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
    [Arguments("personal-details")]
    [Arguments("create-reason")]
    public async Task Get_WhenLinkedToFromFromCheckAnswersPage_BacklinkLinksToCheckAnswersPage(string page)
    {
        // Arrange
        var firstName = "Alfred";
        var middleName = "The";
        var lastName = "Great";
        var dateOfBirth = DateOnly.Parse("1 Feb 1980");
        var emailAddress = "some@email-address.com";
        var nationalInsuranceNumber = "AB123456D";

        var state = new CreateStateBuilder()
            .WithInitializedState()
            .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithCreateReasonChoice(CreateReasonOption.MandatoryQualification)
            .WithUploadEvidenceChoice(false);

        var journeyInstance = await CreateJourneyInstanceAsync(state.Build());
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/create/{page}?FromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/create/check-answers", backlink!.Href);
    }

    [Test]
    [Arguments("personal-details")]
    [Arguments("create-reason")]
    public async Task Post_WhenLinkedToFromCheckAnswersPage_AndMoreChangesMade_RedirectsToCheckAnswersPage(string page)
    {
        // Arrange
        var firstName = "Alfred";
        var middleName = "The";
        var lastName = "Great";
        var dateOfBirth = DateOnly.Parse("1 Feb 1980");
        var emailAddress = "some@email-address.com";
        var mobileNumber = "07891234567";
        var nationalInsuranceNumber = "AB123456D";

        var state = new CreateStateBuilder()
            .WithInitializedState()
            .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithCreateReasonChoice(CreateReasonOption.MandatoryQualification)
            .WithUploadEvidenceChoice(false);

        var journeyInstance = await CreateJourneyInstanceAsync(state.Build());
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/create/{page}?fromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new CreatePostRequestContentBuilder()
                .WithFirstName(firstName)
                .WithMiddleName(middleName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithEmailAddress(emailAddress)
                .WithMobileNumber(mobileNumber)
                .WithNationalInsuranceNumber(nationalInsuranceNumber)
                .WithCreateReason(CreateReasonOption.MandatoryQualification)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;

        Assert.Equal($"/persons/create/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", location);
    }

    public static IEnumerable<(string Page, string? Role)> GetPagesForUserWithoutPersonDataEditPermissionData()
    {
        var pages = new[] { "personal-details", "create-reason", "check-answers" };

        var rolesWithoutWritePermission = new[] { UserRoles.Viewer }
            .Append(null)
            .ToArray();

        var data = new List<(string Page, string? Role)>();

        foreach (var page in pages)
        {
            foreach (var role in rolesWithoutWritePermission)
            {
                data.Add((page, role));
            }
        }

        return data;
    }

    private Task<JourneyInstance<CreateState>> CreateJourneyInstanceAsync(CreateState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.CreatePerson,
            state ?? new CreateState());
}
