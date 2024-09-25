using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.StartDate;

public class ReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithAlert());
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_ReturnsOK()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var journeyStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertStartDateState()
            {
                Initialized = true,
                StartDate = journeyStartDate
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithAlert());
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenNoChangeReasonIsSelected_ReturnsError()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var journeyStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertStartDateState()
            {
                Initialized = true,
                StartDate = journeyStartDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var journeyStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertStartDateState()
            {
                Initialized = true,
                StartDate = journeyStartDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["ChangeReason"] = "AnotherReason",
                ["ChangeReasonDetail"] = "",
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
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var journeyStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertStartDateState()
            {
                Initialized = true,
                StartDate = journeyStartDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["ChangeReason"] = "AnotherReason",
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
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var journeyStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertStartDateState()
            {
                Initialized = true,
                StartDate = journeyStartDate
            });

        var multipartContent = CreateFormFileUpload(".cs");
        multipartContent.Add(new StringContent(AlertChangeStartDateReasonOption.AnotherReason.ToString()), "ChangeReason");
        multipartContent.Add(new StringContent("My change reason detail"), "ChangeReasonDetail");
        multipartContent.Add(new StringContent("True"), "UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var journeyStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertStartDateState()
            {
                Initialized = true,
                StartDate = journeyStartDate
            });

        var multipartContent = CreateFormFileUpload(".pdf");
        multipartContent.Add(new StringContent(AlertChangeStartDateReasonOption.AnotherReason.ToString()), "ChangeReason");
        multipartContent.Add(new StringContent("My change reason detail"), "ChangeReasonDetail");
        multipartContent.Add(new StringContent("True"), "UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
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
    private async Task<JourneyInstance<EditAlertStartDateState>> CreateJourneyInstance(Guid alertId, EditAlertStartDateState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditAlertStartDate,
            state ?? new EditAlertStartDateState(),
            new KeyValuePair<string, object>("alertId", alertId));
}
