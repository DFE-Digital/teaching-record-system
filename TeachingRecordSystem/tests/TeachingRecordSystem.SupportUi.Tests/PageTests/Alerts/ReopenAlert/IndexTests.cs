using TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.ReopenAlert;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsReadWrite, UserRoles.DbsAlertsReadWrite));
    }

    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/reopen?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WhenAlertHasNoEndDateSet_ReturnsBadRequest()
    {
        // Arrange
        var startDate = Clock.Today.AddDays(-50);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/reopen?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_ReturnsOk()
    {
        // Arrange
        var startDate = TestData.Clock.Today.AddDays(-50);
        var endDate = TestData.Clock.Today.AddDays(-10);
        var changeReason = ReopenAlertReasonOption.AnotherReason;
        var changeReasonDetail = "Some details";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";
        var evidenceFileSizeDescription = "1 MB";
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(endDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new ReopenAlertState()
            {
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = true,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName,
                EvidenceFileSizeDescription = evidenceFileSizeDescription
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/reopen?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var changeReasons = doc.GetElementsByName("ChangeReason");
        var selectedChangeReason = changeReasons.Single(r => r.HasAttribute("checked"));
        Assert.Equal(changeReason.ToString(), selectedChangeReason.GetAttribute("value"));
        Assert.Equal(changeReasonDetail, doc.GetElementById("ChangeReasonDetail")!.TextContent);
        var uploadEvidence = doc.GetElementsByName("UploadEvidence");
        var selectedUploadEvidenceOption = uploadEvidence.Single(r => r.HasAttribute("checked"));
        Assert.Equal("True", selectedUploadEvidenceOption.GetAttribute("value"));
        var uploadedEvidenceLink = doc.GetElementByTestId("uploaded-evidence-link");
        Assert.NotNull(uploadedEvidenceLink);
        Assert.Equal($"{evidenceFileName} ({evidenceFileSizeDescription})", uploadedEvidenceLink!.TextContent);
    }

    [Fact]
    public async Task Post_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/reopen?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenAlertHasNoEndDateSet_ReturnsBadRequest()
    {
        // Arrange
        var startDate = Clock.Today.AddDays(-50);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/reopen?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenNoChangeReasonIsSelected_ReturnsError()
    {
        // Arrange
        var startDate = Clock.Today.AddDays(-50);
        var endDate = Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(endDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/reopen?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["UploadEvidence"] = "False"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "ChangeReason", "Select a reason");
    }

    [Fact]
    public async Task Post_WhenChangeReasonAnotherReasonIsSelectedAndDetailsAreEmpty_ReturnsError()
    {
        // Arrange
        var startDate = Clock.Today.AddDays(-50);
        var endDate = Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(endDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/reopen?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["ChangeReason"] = ReopenAlertReasonOption.AnotherReason.ToString(),
                ["UploadEvidence"] = "False"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "ChangeReasonDetail", "Enter details");
    }

    [Fact]
    public async Task Post_WhenUploadEvidenceOptionIsYesAndNoFileIsSelected_ReturnsError()
    {
        // Arrange
        var startDate = Clock.Today.AddDays(-50);
        var endDate = Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(endDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/reopen?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["ChangeReason"] = ReopenAlertReasonOption.AnotherReason.ToString(),
                ["ChangeReasonDetail"] = "Some details",
                ["UploadEvidence"] = "True"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EvidenceFile", "Select a file");
    }

    [Fact]
    public async Task Post_WhenEvidenceFileIsInvalidType_ReturnsError()
    {
        // Arrange
        var startDate = Clock.Today.AddDays(-50);
        var endDate = Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(endDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var multipartContent = CreateFormFileUpload(".cs");
        multipartContent.Add(new StringContent(ReopenAlertReasonOption.AnotherReason.ToString()), "ChangeReason");
        multipartContent.Add(new StringContent("My change reason detail"), "ChangeReasonDetail");
        multipartContent.Add(new StringContent("True"), "UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/reopen?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Fact]
    public async Task Post_WhenValidInput_RedirectsToCheckAnswersPage()
    {
        // Arrange
        var startDate = Clock.Today.AddDays(-50);
        var endDate = Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(endDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var multipartContent = CreateFormFileUpload(".pdf");
        multipartContent.Add(new StringContent(ReopenAlertReasonOption.AnotherReason.ToString()), "ChangeReason");
        multipartContent.Add(new StringContent("My change reason detail"), "ChangeReasonDetail");
        multipartContent.Add(new StringContent("True"), "UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/reopen?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/reopen/check-answers", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var startDate = Clock.Today.AddDays(-50);
        var endDate = Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(endDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/reopen/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/alerts/{alertId}", response.Headers.Location!.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private MultipartFormDataContent CreateFormFileUpload(string fileExtension)
    {
        var byteArrayContent = new ByteArrayContent(new byte[] { });
        byteArrayContent.Headers.Add("Content-Type", "application/octet-stream");

        var multipartContent = new MultipartFormDataContent
        {
            { byteArrayContent, "EvidenceFile", $"evidence{fileExtension}" }
        };

        return multipartContent;
    }

    private async Task<JourneyInstance<ReopenAlertState>> CreateJourneyInstance(Guid alertId, ReopenAlertState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.ReopenAlert,
            state ?? new ReopenAlertState(),
            new KeyValuePair<string, object>("alertId", alertId));
}
