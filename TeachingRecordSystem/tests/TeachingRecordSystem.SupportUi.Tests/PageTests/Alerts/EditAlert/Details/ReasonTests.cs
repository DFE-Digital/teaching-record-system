using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.Details;

public class ReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                Details = journeyDetails,
                CurrentDetails = databaseDetails
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var databaseEndDate = new DateOnly(2022, 11, 6);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithAlert());
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsOk()
    {
        // Arrange
        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                Details = journeyDetails,
                CurrentDetails = databaseDetails
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithReasonPopulatedDataInJourneyState_ReturnsExpectedContent()
    {
        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var reason = AlertChangeDetailsReasonOption.IncorrectDetails;
        var reasonDetail = "My Reason";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileSizeDescription = "1 MB";

        var journeyInstance = await CreateJourneyInstance(alertId, state: new()
        {
            Initialized = true,
            CurrentDetails = databaseDetails,
            Details = journeyDetails,
            ChangeReason = reason,
            HasAdditionalReasonDetail = true,
            ChangeReasonDetail = reasonDetail,
            UploadEvidence = true,
            EvidenceFileId = evidenceFileId,
            EvidenceFileName = evidenceFileName,
            EvidenceFileSizeDescription = evidenceFileSizeDescription,
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        AssertCheckedRadioOption("ChangeReason", reason.ToString());
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
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                Details = journeyDetails,
                CurrentDetails = databaseDetails
            });

        var reason = AlertChangeDetailsReasonOption.AnotherReason;
        var HasAdditionalReasonDetail = true;
        var reasonDetail = "More details";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", reason },
                { "HasAdditionalReasonDetail", HasAdditionalReasonDetail },
                { "ChangeReasonDetail", reasonDetail },
                { "UploadEvidence", bool.FalseString }
            }
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
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var databaseEndDate = new DateOnly(2022, 11, 6);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_MissingDataInJourneyState_RedirectsToIndexPage()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithAlert());
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", AddAlertReasonOption.AnotherReason },
                { "HasAdditionalReasonDetail", bool.FalseString },
                { "UploadEvidence", bool.FalseString }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/details?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenNoChangeReasonIsSelected_ReturnsError()
    {
        // Arrange
        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                Details = journeyDetails,
                CurrentDetails = databaseDetails
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        await AssertEx.HtmlResponseHasError(response, "ChangeReason", "Select a reason");
    }

    [Fact]
    public async Task Post_WhenNoHasAdditionalReasonDetailOptionIsSelected_ReturnsError()
    {
        // Arrange
        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                Details = journeyDetails,
                CurrentDetails = databaseDetails
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", AlertChangeDetailsReasonOption.ChangeOfDetails },
                { "UploadEvidence", bool.FalseString }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "HasAdditionalReasonDetail", "Select yes if you want to add more information about why you’re changing the alert details");
    }

    [Fact]
    public async Task Post_WhenHasAdditionalDetailIsYesButAdditionalDetailsAreEmpty_ReturnsError()
    {
        // Arrange
        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                Details = journeyDetails,
                CurrentDetails = databaseDetails
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", AlertChangeDetailsReasonOption.ChangeOfDetails },
                { "HasAdditionalReasonDetail", bool.TrueString },
                { "UploadEvidence", bool.FalseString }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "ChangeReasonDetail", "Enter additional detail");
    }

    [Fact]
    public async Task Post_WhenUploadEvidenceOptionIsYesAndNoFileIsSelected_ReturnsError()
    {
        // Arrange
        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                Details = journeyDetails,
                CurrentDetails = databaseDetails
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", AlertChangeDetailsReasonOption.ChangeOfDetails },
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
        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                Details = journeyDetails,
                CurrentDetails = databaseDetails
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", AlertChangeDetailsReasonOption.ChangeOfDetails },
                { "HasAdditionalReasonDetail", bool.FalseString },
                { "EvidenceFile", CreateEvidenceFileBinaryContent(), "invalidfile.cs" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Fact]
    public async Task Post_ValidInputWithoutEvidenceFile_UpdatesStateAndRedirectsToCheckAnswersPage()
    {
        // Arrange
        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                Details = journeyDetails,
                CurrentDetails = databaseDetails
            });

        var reason = AlertChangeDetailsReasonOption.IncorrectDetails;
        var HasAdditionalReasonDetail = true;
        var reasonDetail = "More details";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", reason },
                { "HasAdditionalReasonDetail", HasAdditionalReasonDetail },
                { "ChangeReasonDetail", reasonDetail },
                { "UploadEvidence", bool.FalseString }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

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
        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                Details = journeyDetails,
                CurrentDetails = databaseDetails
            });

        var reason = AlertChangeDetailsReasonOption.AnotherReason;
        var HasAdditionalReasonDetail = false;
        var evidenceFileName = "evidence.pdf";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", reason },
                { "HasAdditionalReasonDetail", HasAdditionalReasonDetail },
                { "UploadEvidence", bool.TrueString },
                { "EvidenceFile", CreateEvidenceFileBinaryContent(), evidenceFileName }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

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
        var databaseDetails = TestData.GenerateLoremIpsum();
        var journeyDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithDetails(databaseDetails)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                Details = journeyDetails,
                CurrentDetails = databaseDetails
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/details/change-reason/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private static HttpContent CreateEvidenceFileBinaryContent()
    {
        var byteArrayContent = new ByteArrayContent([]);
        byteArrayContent.Headers.Add("Content-Type", "application/octet-stream");
        return byteArrayContent;
    }

    private Task<JourneyInstance<EditAlertDetailsState>> CreateJourneyInstance(Guid alertId, string currentDetails) =>
        CreateJourneyInstance(
            alertId,
            new EditAlertDetailsState()
            {
                Initialized = true,
                CurrentDetails = currentDetails,
                Details = currentDetails
            });

    private async Task<JourneyInstance<EditAlertDetailsState>> CreateJourneyInstance(Guid alertId, EditAlertDetailsState state) =>
        await CreateJourneyInstance(
            JourneyNames.EditAlertDetails,
            state,
            new KeyValuePair<string, object>("alertId", alertId));
}
