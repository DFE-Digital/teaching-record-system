using TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.DeleteAlert;

public class IndexTests(HostFixture hostFixture) : DeleteAlertTestBase(hostFixture)
{
    private const string PreviousStep = JourneySteps.New;
    private const string ThisStep = JourneySteps.Index;

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Get_ValidRequest_ReturnsOk(bool isOpenAlert)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert() : await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_ReturnsExpectedContent(bool isOpenAlert)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert() : await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(ThisStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        AssertCheckedRadioOption("HasAdditionalReasonDetail", bool.TrueString);
        Assert.Equal(journeyInstance.State.DeleteReasonDetail, doc.GetElementsByName("DeleteReasonDetail")[0].TrimmedText());
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

        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreateMinimumValidPostContent()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode); ;
    }

    [Test]
    public async Task Post_AlertDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_WhenNoChangeReasonIsSelected_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(changeReason: null, hasAdditionalReasonDetail: false, uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "DeleteReason", "Select a reason");
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Post_NoHasAdditionalReasonDetailIsSelected_ReturnsError(bool isOpenAlert)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert() : await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "HasAdditionalReasonDetail", "Select yes if you want to add more information");
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Post_AdditionalDetailIsYesButAdditionalDetailsAreEmpty_ReturnsError(bool isOpenAlert)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert() : await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                hasAdditionalReasonDetail: true,
                deleteReasonDetail: null,
                uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "DeleteReasonDetail", "Enter additional detail");
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Post_WhenUploadEvidenceOptionIsYesAndNoFileIsSelected_ReturnsError(bool isOpenAlert)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert() : await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
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
    [Arguments(true)]
    [Arguments(false)]
    public async Task Post_WhenEvidenceFileIsInvalidType_ReturnsError(bool isOpenAlert)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert() : await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                hasAdditionalReasonDetail: false,
                uploadEvidence: true,
                evidenceFile: (CreateEvidenceFileBinaryContent(), "invalidfile.cs"))
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Post_ValidInputWithoutEvidenceFile_UpdatesStateAndRedirectsToCheckAnswersPage(bool isOpenAlert)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert() : await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var deleteReason = DeleteAlertReasonOption.AddedInError;
        var hasAdditionalReasonDetail = true;
        var reasonDetail = "More details";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                deleteReason,
                hasAdditionalReasonDetail: hasAdditionalReasonDetail,
                deleteReasonDetail: reasonDetail,
                uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(hasAdditionalReasonDetail, journeyInstance.State.HasAdditionalReasonDetail);
        Assert.Equal(reasonDetail, journeyInstance.State.DeleteReasonDetail);
        Assert.Null(journeyInstance.State.Evidence.UploadedEvidenceFile);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Post_ValidInputWithEvidenceFile_UpdatesStateAndRedirectsToCheckAnswersPage(bool isOpenAlert)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert() : await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var deleteReason = DeleteAlertReasonOption.AddedInError;
        var hasAdditionalReasonDetail = false;
        var evidenceFileName = "evidence.pdf";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                deleteReason,
                hasAdditionalReasonDetail: hasAdditionalReasonDetail,
                uploadEvidence: true,
                evidenceFile: (CreateEvidenceFileBinaryContent(), evidenceFileName))
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.False(journeyInstance.State.HasAdditionalReasonDetail);
        Assert.Null(journeyInstance.State.DeleteReasonDetail);
        Assert.Equal(evidenceFileName, journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName);
        Assert.NotNull(journeyInstance.State.Evidence.UploadedEvidenceFile!.FileId);
        Assert.NotNull(journeyInstance.State.Evidence.UploadedEvidenceFile!.FileSizeDescription);
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task Post_Cancel_DeletesJourneyAndRedirects(bool isOpenAlert)
    {
        // Arrange
        var (person, alert) = isOpenAlert ? await CreatePersonWithOpenAlert() : await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        if (isOpenAlert)
        {
            Assert.Equal($"/persons/{person.PersonId}/alerts", response.Headers.Location!.OriginalString);
        }
        else
        {
            Assert.Equal($"/alerts/{alert.AlertId}", response.Headers.Location!.OriginalString);
        }

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

        var request = new HttpRequestMessage(httpMethod, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private static MultipartFormDataContentBuilder CreateMinimumValidPostContent() =>
        CreatePostContent(
            hasAdditionalReasonDetail: false,
            uploadEvidence: false);

    private static MultipartFormDataContentBuilder CreatePostContent(
        DeleteAlertReasonOption? changeReason = null,
        bool? hasAdditionalReasonDetail = null,
        string? deleteReasonDetail = null,
        bool? uploadEvidence = null,
        (HttpContent Content, string FileName)? evidenceFile = null)
    {
        return new MultipartFormDataContentBuilder
        {
            { "DeleteReason", changeReason },
            { "HasAdditionalReasonDetail", hasAdditionalReasonDetail },
            { "DeleteReasonDetail", deleteReasonDetail },
            { "Evidence.UploadEvidence", uploadEvidence },
            { "Evidence.EvidenceFile", evidenceFile }
        };
    }
}
