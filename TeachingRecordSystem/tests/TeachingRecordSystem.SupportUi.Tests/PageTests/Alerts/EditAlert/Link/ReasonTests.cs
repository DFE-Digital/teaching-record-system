using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.Link;

public class ReasonTests : LinkTestBase
{
    private const string PreviousStep = JourneySteps.Index;
    private const string ThisStep = JourneySteps.Reason;

    public ReasonTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTraDbs));
    }

    [Theory]
    [RolesWithoutAlertWritePermissionData]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsOk()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        AssertCheckedRadioOption("ChangeReason", journeyInstance.State.ChangeReason!.ToString()!);
        AssertCheckedRadioOption("HasAdditionalReasonDetail", bool.TrueString);
        Assert.Equal(journeyInstance.State.ChangeReasonDetail, doc.GetElementsByName("ChangeReasonDetail")[0].TrimmedText());
        AssertCheckedRadioOption("UploadEvidence", bool.TrueString);

        var uploadedEvidenceLink = doc.GetElementByTestId("uploaded-evidence-link");
        Assert.NotNull(uploadedEvidenceLink);
        Assert.Equal($"{journeyInstance.State.EvidenceFileName} ({journeyInstance.State.EvidenceFileSizeDescription})", uploadedEvidenceLink!.TrimmedText());

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
        SetCurrentUser(TestUsers.GetUser(role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(JourneySteps.New, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreateMinimumValidPostContent()
        };

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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreateMinimumValidPostContent()
        };

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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreateMinimumValidPostContent()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenNoChangeReasonIsSelected_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(changeReason: null, hasAdditionalReasonDetail: false, uploadEvidence: false)
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: AlertChangeLinkReasonOption.AnotherReason,
                uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "HasAdditionalReasonDetail", "Select yes if you want to add more information about why you’re changing the panel outcome link");
    }

    [Fact]
    public async Task Post_WhenHasAdditionalDetailIsYesButAdditionalDetailsAreEmpty_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: AlertChangeLinkReasonOption.AnotherReason,
                hasAdditionalReasonDetail: true,
                changeReasonDetail: null,
                uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ChangeReasonDetail", "Enter additional detail");
    }

    [Fact]
    public async Task Post_WhenUploadEvidenceOptionIsYesAndNoFileIsSelected_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: AlertChangeLinkReasonOption.AnotherReason,
                hasAdditionalReasonDetail: false,
                uploadEvidence: true,
                evidenceFile: null)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EvidenceFile", "Select a file");
    }

    [Fact]
    public async Task Post_WhenEvidenceFileIsInvalidType_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: AlertChangeLinkReasonOption.AnotherReason,
                hasAdditionalReasonDetail: false,
                uploadEvidence: true,
                evidenceFile: (CreateEvidenceFileBinaryContent(), "badevidence.exe"))
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Fact]
    public async Task Post_ValidInputWithoutEvidenceFile_UpdatesStateAndRedirectsToCheckAnswersPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var reason = AlertChangeLinkReasonOption.AnotherReason;
        var hasAdditionalReasonDetail = true;
        var reasonDetail = "More details";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: reason,
                hasAdditionalReasonDetail: hasAdditionalReasonDetail,
                changeReasonDetail: reasonDetail,
                uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/link/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(reason, journeyInstance.State.ChangeReason);
        Assert.True(journeyInstance.State.HasAdditionalReasonDetail);
        Assert.Equal(reasonDetail, journeyInstance.State.ChangeReasonDetail);
        Assert.Null(journeyInstance.State.EvidenceFileName);
        Assert.Null(journeyInstance.State.EvidenceFileId);
        Assert.Null(journeyInstance.State.EvidenceFileSizeDescription);
    }

    [Fact]
    public async Task Post_WhenValidInputWithEvidenceFile_UpdatesStateAndRedirectsToCheckAnswersPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var reason = AlertChangeLinkReasonOption.AnotherReason;
        var hasAdditionalReasonDetail = false;
        var evidenceFileName = "evidence.pdf";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: reason,
                hasAdditionalReasonDetail: hasAdditionalReasonDetail,
                uploadEvidence: true,
                evidenceFile: (CreateEvidenceFileBinaryContent(), evidenceFileName))
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/link/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(reason, journeyInstance.State.ChangeReason);
        Assert.False(journeyInstance.State.HasAdditionalReasonDetail);
        Assert.Null(journeyInstance.State.ChangeReasonDetail);
        Assert.Equal(evidenceFileName, journeyInstance.State.EvidenceFileName);
        Assert.NotNull(journeyInstance.State.EvidenceFileId);
        Assert.NotNull(journeyInstance.State.EvidenceFileSizeDescription);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/change-reason/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private static MultipartFormDataContentBuilder CreateMinimumValidPostContent() =>
        CreatePostContent(
            changeReason: AlertChangeLinkReasonOption.AnotherReason,
            hasAdditionalReasonDetail: false,
            uploadEvidence: false);

    private static MultipartFormDataContentBuilder CreatePostContent(
        AlertChangeLinkReasonOption? changeReason = null,
        bool? hasAdditionalReasonDetail = null,
        string? changeReasonDetail = null,
        bool? uploadEvidence = null,
        (HttpContent Content, string FileName)? evidenceFile = null)
    {
        var builder = new MultipartFormDataContentBuilder();

        if (changeReason is not null)
        {
            builder.Add("ChangeReason", changeReason);
        }

        if (hasAdditionalReasonDetail is not null)
        {
            builder.Add("HasAdditionalReasonDetail", hasAdditionalReasonDetail);
        }

        if (changeReasonDetail is not null)
        {
            builder.Add("ChangeReasonDetail", changeReasonDetail);
        }

        if (uploadEvidence is not null)
        {
            builder.Add("UploadEvidence", uploadEvidence);
        }

        if (evidenceFile is not null)
        {
            builder.Add("EvidenceFile", evidenceFile.Value.Content, evidenceFile.Value.FileName);
        }

        return builder;
    }
}
