using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public class ReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(personId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/reason?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToStartDatePage()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false
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
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();
        var reason = AddAlertReasonOption.AnotherReason;
        var reasonDetail = "My Reason";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileSizeDescription = "1 MB";

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
            StartDate = new DateOnly(2022, 1, 1),
            AddReason = reason,
            HasAdditionalReasonDetail = true,
            AddReasonDetail = reasonDetail,
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

        AssertCheckedRadioOption("AddReason", reason.ToString());
        AssertCheckedRadioOption("HasAdditionalReasonDetail", bool.TrueString);
        AssertCheckedRadioOption("UploadEvidence", bool.TrueString);

        var uploadedEvidenceLink = doc.GetElementByTestId("uploaded-evidence-link");
        Assert.NotNull(uploadedEvidenceLink);
        Assert.Equal($"{evidenceFileName} ({evidenceFileSizeDescription})", uploadedEvidenceLink!.TextContent);

        void AssertCheckedRadioOption(string name, string expectedCheckedValue)
        {
            var selectedOption = doc.GetElementsByName(name).SingleOrDefault(r => r.HasAttribute("checked"));
            Assert.Equal(expectedCheckedValue, selectedOption?.GetAttribute("value"));
        }
    }

    [Fact]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(personId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "AddReason", AddAlertReasonOption.AnotherReason },
                { "HasAdditionalReasonDetail", bool.FalseString },
                { "UploadEvidence", bool.FalseString }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenNoReasonIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "HasAdditionalReasonDetail", bool.FalseString },
                { "UploadEvidence", bool.FalseString }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "AddReason", "Select a reason");
    }

    [Fact]
    public async Task Post_WhenNoHasAdditionalReasonDetailIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "AddReason", AddAlertReasonOption.AnotherReason },
                { "UploadEvidence", bool.FalseString }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "HasAdditionalReasonDetail", "Select yes if you want to add more information about why you’re adding this alert");
    }

    [Fact]
    public async Task Post_WhenNoUploadEvidenceOptionIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "AddReason", AddAlertReasonOption.AnotherReason },
                { "HasAdditionalReasonDetail", bool.FalseString }
            }
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
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "AddReason", AddAlertReasonOption.AnotherReason },
                { "HasAdditionalReasonDetail", bool.FalseString },
                { "UploadEvidence", bool.TrueString }
            }
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
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "AddReason", AddAlertReasonOption.AnotherReason },
                { "HasAdditionalReasonDetail", bool.FalseString },
                { "UploadEvidence", bool.TrueString },
                { "EvidenceFile", CreateEvidenceFileBinaryContent(), "badfile.exe"}
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Fact]
    public async Task Post_AdditionalReasonIsTooLong_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "AddReason", AddAlertReasonOption.AnotherReason },
                { "HasAdditionalReasonDetail", bool.TrueString },
                { "AddReasonDetail", new string('x', 4001) },
                { "UploadEvidence", bool.FalseString }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "AddReasonDetail", "Additional detail must be 4000 characters or less");
    }

    [Fact]
    public async Task Post_ValidInputWithoutEvidenceFile_UpdatesStateAndRedirectsToCheckAnswersPage()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "AddReason", AddAlertReasonOption.AnotherReason },
                { "HasAdditionalReasonDetail", bool.FalseString },
                { "UploadEvidence", bool.FalseString }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/check-answers?personId={person.PersonId}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance.State.EvidenceFileName);
        Assert.Null(journeyInstance.State.EvidenceFileId);
        Assert.Null(journeyInstance.State.EvidenceFileSizeDescription);
    }

    [Fact]
    public async Task Post_ValidInputWithEvidenceFile_UpdatesStateAndRedirectsToCheckAnswersPage()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();
        var evidenceFileName = "evidence.pdf";

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState()
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
            StartDate = new DateOnly(2022, 1, 1)
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "AddReason", AddAlertReasonOption.AnotherReason },
                { "HasAdditionalReasonDetail", bool.FalseString },
                { "UploadEvidence", bool.TrueString },
                { "EvidenceFile", CreateEvidenceFileBinaryContent(), evidenceFileName}
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/check-answers?personId={person.PersonId}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(evidenceFileName, journeyInstance.State.EvidenceFileName);
        Assert.NotNull(journeyInstance.State.EvidenceFileId);
        Assert.NotNull(journeyInstance.State.EvidenceFileSizeDescription);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).RandomOne();

        var journeyInstance = await CreateJourneyInstance(person.PersonId, new AddAlertState
        {
            AlertTypeId = alertType.AlertTypeId,
            AlertTypeName = alertType.Name,
            Details = "Details",
            AddLink = false,
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

    private static HttpContent CreateEvidenceFileBinaryContent()
    {
        var byteArrayContent = new ByteArrayContent([]);
        byteArrayContent.Headers.Add("Content-Type", "application/octet-stream");
        return byteArrayContent;
    }

    private async Task<JourneyInstance<AddAlertState>> CreateJourneyInstance(Guid personId, AddAlertState? state = null) =>
        await CreateJourneyInstance(
             JourneyNames.AddAlert,
             state ?? new AddAlertState(),
             new KeyValuePair<string, object>("personId", personId));
}
