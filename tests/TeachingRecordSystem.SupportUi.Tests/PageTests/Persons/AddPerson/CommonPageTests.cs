using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.AddPerson;

public class CommonPageTests(HostFixture hostFixture) : AddPersonTestBase(hostFixture)
{
    [Theory]
    [MemberData(nameof(GetPagesForUserWithoutPersonDataEditPermissionData))]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden(string page, string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"/persons/add/{page}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [InlineData("/reason", "/personal-details")]
    [InlineData("/check-answers", "/reason")]
    public async Task Get_BacklinkContainsExpected(string fromPage, string expectedBackPage)
    {
        // Arrange
        var firstName = "Alfred";
        var middleName = "The";
        var lastName = "Great";
        var dateOfBirth = DateOnly.Parse("1 Feb 1980");
        var emailAddress = "some@email-address.com";
        var nationalInsuranceNumber = "AB123456D";

        var state = new AddPersonStateBuilder()
                        .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithAddPersonReasonChoice(PersonCreateReason.MandatoryQualification)
            .WithUploadEvidenceChoice(false);

        var journeyInstance = await CreateJourneyInstanceAsync(state.Build());
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/add{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        Assert.Contains($"/persons/add{expectedBackPage}", backlink.Href);
    }

    [Theory]
    [InlineData("/personal-details", "/reason")]
    [InlineData("/reason", "/check-answers")]
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

        var state = new AddPersonStateBuilder()
                        .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithAddPersonReasonChoice(PersonCreateReason.MandatoryQualification)
            .WithUploadEvidenceChoice(false);

        var journeyInstance = await CreateJourneyInstanceAsync(state.Build());
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/add{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithFirstName(firstName)
                .WithMiddleName(middleName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithEmailAddress(emailAddress)
                .WithMobileNumber(mobileNumber)
                .WithNationalInsuranceNumber(nationalInsuranceNumber)
                .WithReason(PersonCreateReason.MandatoryQualification)
                .WithUploadEvidence(false)
                .WithAdditionalInformation(ProvideMoreInformationOption.Yes, "Some more information")
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/add{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Theory]
    [InlineData("personal-details")]
    [InlineData("reason")]
    [InlineData("check-answers")]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToAddPersonIndexPage(string page)
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithAddPersonReasonChoice(PersonCreateReason.MandatoryQualification)
                .WithUploadEvidenceChoice(false)
                .Build());

        var pageUrl = $"/persons/add/{page}?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;
        Assert.NotNull(cancelButton);
        Assert.Equal("Cancel", cancelButton.Name);

        // Act
        var cancelRequest = new HttpRequestMessage(HttpMethod.Post, pageUrl)
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };
        var cancelResponse = await HttpClient.SendAsync(cancelRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)cancelResponse.StatusCode);
        var location = cancelResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/add", location);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Theory]
    [InlineData("personal-details")]
    [InlineData("reason")]
    [InlineData("check-answers")]
    public async Task Post_Cancel_EvidenceFilePreviouslyUploaded_DeletesPreviouslyUploadedFile(string page)
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithAddPersonReasonChoice(PersonCreateReason.MandatoryQualification)
                .WithUploadEvidenceChoice(true, evidenceFileId, "evidence.jpg", "1.2 KB")
                .Build());

        var pageUrl = $"/persons/add/{page}?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        // Act
        var cancelRequest = new HttpRequestMessage(HttpMethod.Post, pageUrl)
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };
        var cancelResponse = await HttpClient.SendAsync(cancelRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)cancelResponse.StatusCode);
        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Theory]
    [InlineData("personal-details")]
    [InlineData("reason")]
    public async Task Get_WithReturnUrlToCheckAnswersPage_BacklinkLinksToCheckAnswersPage(string page)
    {
        // Arrange
        var firstName = "Alfred";
        var middleName = "The";
        var lastName = "Great";
        var dateOfBirth = DateOnly.Parse("1 Feb 1980");
        var emailAddress = "some@email-address.com";
        var nationalInsuranceNumber = "AB123456D";

        var state = new AddPersonStateBuilder()
                        .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithAddPersonReasonChoice(PersonCreateReason.MandatoryQualification)
            .WithUploadEvidenceChoice(false);

        var journeyInstance = await CreateJourneyInstanceAsync(state.Build());
        var checkAnswersUrl = $"/persons/add/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/add/{page}?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/add/check-answers", backlink!.Href);
    }

    [Theory]
    [InlineData("personal-details")]
    [InlineData("reason")]
    public async Task Post_WithReturnUrlToCheckAnswersPage_RedirectsToCheckAnswersPage(string page)
    {
        // Arrange
        var firstName = "Alfred";
        var middleName = "The";
        var lastName = "Great";
        var dateOfBirth = DateOnly.Parse("1 Feb 1980");
        var emailAddress = "some@email-address.com";
        var mobileNumber = "07891234567";
        var nationalInsuranceNumber = "AB123456D";

        var state = new AddPersonStateBuilder()
                        .WithName(firstName, middleName, lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(emailAddress)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithAddPersonReasonChoice(PersonCreateReason.MandatoryQualification)
            .WithUploadEvidenceChoice(false);

        var journeyInstance = await CreateJourneyInstanceAsync(state.Build());
        var checkAnswersUrl = $"/persons/add/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/add/{page}?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithFirstName(firstName)
                .WithMiddleName(middleName)
                .WithLastName(lastName)
                .WithDateOfBirth(dateOfBirth)
                .WithEmailAddress(emailAddress)
                .WithMobileNumber(mobileNumber)
                .WithNationalInsuranceNumber(nationalInsuranceNumber)
                .WithReason(PersonCreateReason.MandatoryQualification)
                .WithUploadEvidence(false)
                .WithAdditionalInformation(ProvideMoreInformationOption.Yes, "Some more information")
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;

        Assert.Equal($"/persons/add/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", location);
    }

    public static IEnumerable<(string Page, string? Role)> GetPagesForUserWithoutPersonDataEditPermissionData()
    {
        var pages = new[] { "personal-details", "reason", "check-answers" };

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

}
