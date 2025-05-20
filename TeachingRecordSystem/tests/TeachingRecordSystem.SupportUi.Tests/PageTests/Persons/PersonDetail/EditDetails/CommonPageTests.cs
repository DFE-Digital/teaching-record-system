using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public class CommonPageTests : TestBase
{
    public CommonPageTests(HostFixture hostFixture) : base(hostFixture)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.NewPersonDetails);
    }

    public override void Dispose()
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.NewPersonDetails);
        base.Dispose();
    }

    [Theory]
    [InlineData("/edit-details")]
    [InlineData("/edit-details/change-reason")]
    [InlineData("/edit-details/check-answers")]
    public async Task Get_FeatureFlagDisabled_ReturnsNotFound(string page)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.NewPersonDetails);

        // Arrange
        var person = await TestData.CreatePersonAsync();

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}{page}");
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);

        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.NewPersonDetails);
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
    [InlineData("/edit-details", "")]
    [InlineData("/edit-details/change-reason", "/edit-details")]
    [InlineData("/edit-details/check-answers", "/edit-details/change-reason")]
    public async Task Get_BacklinkContainsExpected(string fromPage, string expectedBackPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithChangeReasonChoice(EditDetailsChangeReasonOption.IncompleteDetails)
                .WithUploadEvidenceChoice(false)
                .Build());
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
    [InlineData("/edit-details", "/edit-details/change-reason")]
    [InlineData("/edit-details/change-reason", "/edit-details/check-answers")]
    public async Task Post_RedirectsToExpectedPage(string fromPage, string expectedNextPageUrl)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithChangeReasonChoice(EditDetailsChangeReasonOption.IncompleteDetails)
                .WithUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(
                new EditDetailsPostRequestBuilder()
                    .WithFirstName("Alfred")
                    .WithLastName("Great")
                    .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                    .WithChangeReason(EditDetailsChangeReasonOption.IncompleteDetails)
                    .WithNoFileUploadSelection()
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = expectedNextPageUrl == "induction"
            ? $"/persons/{person.PersonId}{expectedNextPageUrl}"
            : $"/persons/{person.PersonId}{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Theory]
    [InlineData("/edit-details", "")]
    [InlineData("/edit-details/change-reason", "")]
    [InlineData("/edit-details/check-answers", "")]
    public async Task Post_Cancel_RedirectsToExpectedPage(string fromPage, string expectedRedirectPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithChangeReasonChoice(EditDetailsChangeReasonOption.IncompleteDetails)
                .WithUploadEvidenceChoice(false)
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
        Assert.Equal($"/persons/{person.PersonId}{expectedRedirectPage}", location);
    }

    [Theory]
    [InlineData("/edit-details")]
    [InlineData("/edit-details/change-reason")]
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
                .WithChangeReasonChoice(EditDetailsChangeReasonOption.IncompleteDetails)
                .WithUploadEvidenceChoice(true, evidenceFileId, "evidence.jpg", "1.2 KB")
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
        var location = redirectResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}", location);

        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Theory]
    [InlineData("/edit-details", "/edit-details/check-answers")]
    [InlineData("/edit-details/change-reason", "/edit-details/check-answers")]
    public async Task Get_WhenLinkedToFromFromCheckAnswersPage_BacklinkContainsExpected(string fromPage, string expectedBackPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .Build());
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
    [InlineData("/edit-details", "/edit-details/check-answers")]
    [InlineData("/edit-details/change-reason", "/edit-details/check-answers")]
    public async Task Post_WhenLinkedToFromCheckAnswersPage_RedirectsToExpectedPage(string page, string expectedNextPageUrl)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}{page}?FromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(
                new EditDetailsPostRequestBuilder()
                    .WithFirstName("Alfred")
                    .WithLastName("Great")
                    .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                    .WithChangeReason(EditDetailsChangeReasonOption.IncompleteDetails)
                    .WithNoFileUploadSelection()
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    public static TheoryData<string, string?> GetPagesForUserWithoutPersonDataEditPermissionData()
    {
        var pages = new[] { "edit-details", "edit-details/change-reason", "edit-details/check-answers" };

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
