using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.StartDate;

public class ReasonTests(HostFixture hostFixture) : StartDateTestBase(hostFixture)
{
    private const string PreviousStep = JourneySteps.Index;
    private const string ThisStep = JourneySteps.Reason;

    [Before(Test)]
    public async Task SetUser() => SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.AlertsManagerTraDbs));

    [Test]
    [RolesWithoutAlertWritePermissionData]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_AlertDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_AlertIsClosed_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_StartDateIsNotYetChanged_RedirectsToIndexPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(JourneySteps.New, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Get_ValidRequest_ReturnsOk()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_ValidRequestWithReasonPopulatedInJourneyState_ReturnsExpectedContent()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(ThisStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        AssertCheckedRadioOption("ChangeReason", journeyInstance.State.ChangeReason!.ToString()!);
        AssertCheckedRadioOption("HasAdditionalReasonDetail", bool.TrueString);
        Assert.Equal(journeyInstance.State.ChangeReasonDetail, doc.GetElementsByName("ChangeReasonDetail")[0].TrimmedText());
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

    [Test]
    [RolesWithoutAlertWritePermissionData]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(JourneySteps.New, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreateMinimumValidPostContent()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_AlertDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreateMinimumValidPostContent()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_AlertIsClosed_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(JourneySteps.New, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreateMinimumValidPostContent()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_StartDateIsNotYetChanged_RedirectsToIndexPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(JourneySteps.New, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreateMinimumValidPostContent()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Post_NoChangeReasonIsSelected_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(changeReason: null, hasAdditionalReasonDetail: false, uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ChangeReason", "Select a reason");
    }

    [Test]
    public async Task Post_NoHasAdditionalReasonDetailIsSelected_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: AlertChangeStartDateReasonOption.AnotherReason,
                uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "HasAdditionalReasonDetail", "Select yes if you want to add more information");
    }

    [Test]
    public async Task Post_AdditionalDetailIsYesButAdditionalDetailsAreEmpty_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: AlertChangeStartDateReasonOption.AnotherReason,
                hasAdditionalReasonDetail: true,
                changeReasonDetail: null,
                uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ChangeReasonDetail", "Enter additional detail");
    }

    [Test]
    public async Task Post_UploadEvidenceOptionIsYesAndNoFileIsSelected_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: AlertChangeStartDateReasonOption.AnotherReason,
                hasAdditionalReasonDetail: false,
                uploadEvidence: true,
                evidenceFile: null)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.EvidenceFile", "Select a file");
    }

    [Test]
    public async Task Post_EvidenceFileIsInvalidType_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                changeReason: AlertChangeStartDateReasonOption.AnotherReason,
                hasAdditionalReasonDetail: false,
                uploadEvidence: true,
                evidenceFile: (CreateEvidenceFileBinaryContent(), "badevidence.exe"))
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Test]
    public async Task Post_ValidInputWithoutEvidenceFile_UpdatesStateAndRedirectsToCheckAnswersPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var reason = AlertChangeStartDateReasonOption.AnotherReason;
        var hasAdditionalReasonDetail = true;
        var reasonDetail = "More details";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.StartsWith($"/alerts/{alert.AlertId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(reason, journeyInstance.State.ChangeReason);
        Assert.Equal(hasAdditionalReasonDetail, journeyInstance.State.HasAdditionalReasonDetail);
        Assert.Equal(reasonDetail, journeyInstance.State.ChangeReasonDetail);
        Assert.Null(journeyInstance.State.Evidence.UploadedEvidenceFile);
    }

    [Test]
    public async Task Post_ValidInputWithEvidenceFile_UpdatesStateAndRedirectsToCheckAnswersPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var reason = AlertChangeStartDateReasonOption.AnotherReason;
        var hasAdditionalReasonDetail = false;
        var evidenceFileName = "evidence.pdf";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.StartsWith($"/alerts/{alert.AlertId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(reason, journeyInstance.State.ChangeReason);
        Assert.False(journeyInstance.State.HasAdditionalReasonDetail);
        Assert.Null(journeyInstance.State.ChangeReasonDetail);
        Assert.Equal(evidenceFileName, journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName);
        Assert.NotNull(journeyInstance.State.Evidence.UploadedEvidenceFile!.FileId);
        Assert.NotNull(journeyInstance.State.Evidence.UploadedEvidenceFile!.FileSizeDescription);
    }

    [Test]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/start-date/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Test]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(httpMethod, $"/alerts/{alert.AlertId}/start-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private static MultipartFormDataContentBuilder CreateMinimumValidPostContent() =>
        CreatePostContent(
            changeReason: AlertChangeStartDateReasonOption.AnotherReason,
            hasAdditionalReasonDetail: false,
            uploadEvidence: false);

    private static MultipartFormDataContentBuilder CreatePostContent(
        AlertChangeStartDateReasonOption? changeReason = null,
        bool? hasAdditionalReasonDetail = null,
        string? changeReasonDetail = null,
        bool? uploadEvidence = null,
        (HttpContent Content, string FileName)? evidenceFile = null)
    {
        return new MultipartFormDataContentBuilder
        {
            { "ChangeReason", changeReason },
            { "HasAdditionalReasonDetail", hasAdditionalReasonDetail },
            { "ChangeReasonDetail", changeReasonDetail },
            { "Evidence.UploadEvidence", uploadEvidence },
            { "Evidence.EvidenceFile", evidenceFile }
        };
    }
}
