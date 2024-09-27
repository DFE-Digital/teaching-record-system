using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public class ReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstance(personId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details",
            StartDate = new DateOnly(2022, 1, 1),
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/reason?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var reason = "My Reason";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileSizeDescription = "1 MB";

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details",
            StartDate = new DateOnly(2021, 1, 1),
            Reason = reason,
            UploadEvidence = true,
            EvidenceFileId = evidenceFileId,
            EvidenceFileName = evidenceFileName,
            EvidenceFileSizeDescription = evidenceFileSizeDescription
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(reason, doc.GetElementById("Detail")?.TextContent);
        var radioButtons = doc.GetElementsByName("UploadEvidence");
        var selectedRadioButton = radioButtons.Single(r => r.HasAttribute("checked"));
        Assert.Equal("True", selectedRadioButton.GetAttribute("value"));
        var uploadedEvidenceLink = doc.GetElementByTestId("uploaded-evidence-link");
        Assert.NotNull(uploadedEvidenceLink);
        Assert.Equal($"{evidenceFileName} ({evidenceFileSizeDescription})", uploadedEvidenceLink!.TextContent);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToEndDatePage()
    {
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details"
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/start-date?personId={person.PersonId}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithPersonIdForValidPerson_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details",
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstance(personId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details",
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Reason"] = "Reason",
                ["UploadEvidence"] = "False"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenNoUploadEvidenceOptionIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details",
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Reason"] = "Reason"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "UploadEvidence", "Select yes if you want to upload evidence");
    }

    [Fact]
    public async Task Post_WhenUploadEvidenceOptionIsYesAndNoFileIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details",
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Reason"] = "Reason",
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
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details",
            StartDate = new DateOnly(2022, 1, 1)
        });

        var multipartContent = CreateFormFileUpload(".cs");
        multipartContent.Add(new StringContent("My reason to add an alert"), "Reason");
        multipartContent.Add(new StringContent("True"), "UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Fact]
    public async Task Post_ValidInput_RedirectsToCheckAnswersPage()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details",
            StartDate = new DateOnly(2022, 1, 1)
        });

        var multipartContent = CreateFormFileUpload(".pdf");
        multipartContent.Add(new StringContent("My reason to add an alert"), "Reason");
        multipartContent.Add(new StringContent("True"), "UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/check-answers?personId={person.PersonId}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = Guid.NewGuid(),
            Details = "Details",
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

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

    private async Task<JourneyInstance<AddAlertState>> CreateJourneyInstance(Guid personId, AddAlertState? state = null) =>
        await CreateJourneyInstance(
             JourneyNames.AddAlert,
             state ?? new AddAlertState(),
             new KeyValuePair<string, object>("personId", personId));
}
