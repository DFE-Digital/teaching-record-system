using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.Details;

public class ReasonTests(HostFixture hostFixture) : DetailsTestBase(hostFixture), IAsyncLifetime
{
    private const string PreviousStep = JourneySteps.Index;
    private const string ThisStep = JourneySteps.Reason;

    async ValueTask IAsyncLifetime.InitializeAsync() => SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.AlertsManagerTraDbs));

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    [Theory]
    [RolesWithoutAlertWritePermissionData]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(JourneySteps.New, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsOk()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithReasonPopulatedDataInJourneyState_ReturnsExpectedContent()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(ThisStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        AssertCheckedRadioOption("ChangeReason", journeyInstance.State.ChangeReason!.ToString()!);
        AssertCheckedRadioOption("ProvideAdditionalInformation", bool.FalseString);
        var reasonDetailTextbox =
            doc.GetElementById("ChangeReasonDetail") as IHtmlInputElement;
        Assert.Equal(journeyInstance.State.ChangeReasonDetail, reasonDetailTextbox!.Value);
        Assert.Empty(doc.GetElementsByName("AdditionalInformation")[0].TrimmedText());
        AssertCheckedRadioOption("Evidence.UploadEvidence", bool.TrueString);

        var uploadedEvidenceLink = doc.GetElementByTestId("uploaded-evidence-file-link");
        Assert.NotNull(uploadedEvidenceLink);
        Assert.Equal($"{journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName} ({journeyInstance.State.Evidence.UploadedEvidenceFile!.FileSizeDescription})", uploadedEvidenceLink!.TrimmedText());

        void AssertCheckedRadioOption(string name, string expectedCheckedValue)
        {
            var selectedOption = doc.GetElementsByName(name).SingleOrDefault(r => r.HasAttribute("checked"));
            Assert.Equal(expectedCheckedValue, selectedOption?.GetAttribute("value"));
        }
    }

    [Theory]
    [RolesWithoutAlertWritePermissionData]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(JourneySteps.New, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreateMinimumValidPostContent()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(JourneySteps.New, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreateMinimumValidPostContent()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenNoChangeReasonIsSelected_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(changeReason: null, provideAdditionalInformation: false, uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ChangeReason", "Select a reason");
    }

    [Fact]
    public async Task Post_WhenNoHasAdditionalReasonDetailOptionIsSelected_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: AlertChangeDetailsReasonOption.AnotherReason,
                uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ProvideAdditionalInformation", "Select yes if you want to add more information");
    }

    [Fact]
    public async Task Post_WhenHasAdditionalDetailIsYesButAdditionalDetailsAreEmpty_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: AlertChangeDetailsReasonOption.AnotherReason,
                provideAdditionalInformation: true,
                additionalInformation: null,
                changeReasonDetail: "some change reason",
                uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "AdditionalInformation", "Enter details");
    }

    [Fact]
    public async Task Post_WhenUploadEvidenceOptionIsYesAndNoFileIsSelected_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: AlertChangeDetailsReasonOption.AnotherReason,
                provideAdditionalInformation: false,
                uploadEvidence: true,
                evidenceFile: null)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.EvidenceFile", "Select a file");
    }

    [Fact]
    public async Task Post_WhenEvidenceFileIsInvalidType_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: AlertChangeDetailsReasonOption.AnotherReason,
                provideAdditionalInformation: false,
                uploadEvidence: true,
                evidenceFile: (CreateEvidenceFileBinaryContent(), "badevidence.exe"))
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Fact]
    public async Task Post_ValidInputWithoutEvidenceFile_UpdatesStateAndRedirectsToCheckAnswersPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var reason = AlertChangeDetailsReasonOption.IncorrectDetails;
        var provideAdditionlInformation = true;
        var additionalInformation = "Additional Information";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: reason,
                provideAdditionalInformation: provideAdditionlInformation,
                additionalInformation: additionalInformation,
                uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        Assert.Equal(reason, journeyInstance.State.ChangeReason);
        Assert.True(journeyInstance.State.ProvideAdditionalInformation);
        Assert.Equal(additionalInformation, journeyInstance.State.AdditionalInformation);
        Assert.Null(journeyInstance.State.ChangeReasonDetail);
        Assert.Null(journeyInstance.State.Evidence.UploadedEvidenceFile);
    }

    [Fact]
    public async Task Post_WhenValidInputWithEvidenceFile_UpdatesStateAndRedirectsToCheckAnswersPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var reason = AlertChangeDetailsReasonOption.AnotherReason;
        var provideAdditionalInformation = false;
        var evidenceFileName = "evidence.pdf";
        var changeReasonDetail = "this is a change reason detail";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: reason,
                changeReasonDetail: changeReasonDetail,
                provideAdditionalInformation: provideAdditionalInformation,
                additionalInformation: null,
                uploadEvidence: true,
                evidenceFile: (CreateEvidenceFileBinaryContent(), evidenceFileName))
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        Assert.Equal(reason, journeyInstance.State.ChangeReason);
        Assert.False(journeyInstance.State.ProvideAdditionalInformation);
        Assert.Equal(changeReasonDetail, journeyInstance.State.ChangeReasonDetail);
        Assert.Null(journeyInstance.State.AdditionalInformation);
        Assert.Equal(evidenceFileName, journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName);
        Assert.NotNull(journeyInstance.State.Evidence.UploadedEvidenceFile!.FileSizeDescription);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder { { "Cancel", bool.TrueString } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(httpMethod, $"/alerts/{alert.AlertId}/details/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private static MultipartFormDataContentBuilder CreateMinimumValidPostContent() =>
        CreatePostContent(
            changeReason: AlertChangeDetailsReasonOption.AnotherReason,
            changeReasonDetail: "some change reason",
            provideAdditionalInformation: false,
            uploadEvidence: false);

    private static MultipartFormDataContentBuilder CreatePostContent(
        AlertChangeDetailsReasonOption? changeReason = null,
        bool? provideAdditionalInformation = null,
        string? additionalInformation = null,
        string? changeReasonDetail = null,
        bool? uploadEvidence = null,
        (HttpContent Content, string FileName)? evidenceFile = null)
    {
        return new MultipartFormDataContentBuilder
        {
            { "ChangeReason", changeReason },
            { "ProvideAdditionalInformation", provideAdditionalInformation },
            { "AdditionalInformation", additionalInformation },
            { "ChangeReasonDetail", changeReasonDetail },
            { "Evidence.UploadEvidence", uploadEvidence },
            { "Evidence.EvidenceFile", evidenceFile }
        };
    }
}
