using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.Link;

public class ReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

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
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var databaseEndDate = new DateOnly(2022, 11, 6);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(databaseStartDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithAlert());
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                AddLink = true,
                Link = TestData.GenerateUrl()
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithReasonPopulatedDataInJourneyState_ReturnsExpectedContent()
    {
        var databaseLink = TestData.GenerateUrl();
        var journeyLink = TestData.GenerateUrl();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithExternalLink(databaseLink)));
        var alertId = person.Alerts.Single().AlertId;
        var reason = AlertChangeLinkReasonOption.IncorrectLink;
        var reasonDetail = "My Reason";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileSizeDescription = "1 MB";

        var journeyInstance = await CreateJourneyInstance(alertId, state: new()
        {
            Initialized = true,
            AddLink = true,
            CurrentLink = databaseLink,
            Link = journeyLink,
            ChangeReason = reason,
            HasAdditionalReasonDetail = true,
            ChangeReasonDetail = reasonDetail,
            UploadEvidence = true,
            EvidenceFileId = evidenceFileId,
            EvidenceFileName = evidenceFileName,
            EvidenceFileSizeDescription = evidenceFileSizeDescription,
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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
    public async Task Post_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId, state: new());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.StartsWith($"/alerts/{alertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenNoChangeReasonIsSelected_ReturnsError()
    {
        // Arrange
        var databaseLink = TestData.GenerateUrl();
        var journeyLink = TestData.GenerateUrl();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithExternalLink(databaseLink)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                AddLink = true,
                CurrentLink = databaseLink,
                Link = journeyLink
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        var databaseLink = TestData.GenerateUrl();
        var journeyLink = TestData.GenerateUrl();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithExternalLink(databaseLink)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                AddLink = true,
                CurrentLink = databaseLink,
                Link = journeyLink
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", AlertChangeLinkReasonOption.ChangeOfLink },
                { "UploadEvidence", bool.FalseString }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "HasAdditionalReasonDetail", "Select yes if you want to add more information about why you’re changing the panel outcome link");
    }

    [Fact]
    public async Task Post_WhenHasAdditionalDetailIsYesButAdditionalDetailsAreEmpty_ReturnsError()
    {
        // Arrange
        var databaseLink = TestData.GenerateUrl();
        var journeyLink = TestData.GenerateUrl();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithExternalLink(databaseLink)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                AddLink = true,
                CurrentLink = databaseLink,
                Link = journeyLink
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", AlertChangeLinkReasonOption.ChangeOfLink },
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
        var databaseLink = TestData.GenerateUrl();
        var journeyLink = TestData.GenerateUrl();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithExternalLink(databaseLink)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                AddLink = true,
                CurrentLink = databaseLink,
                Link = journeyLink
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", AlertChangeLinkReasonOption.ChangeOfLink },
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
        var databaseLink = TestData.GenerateUrl();
        var journeyLink = TestData.GenerateUrl();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithExternalLink(databaseLink)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                AddLink = true,
                CurrentLink = databaseLink,
                Link = journeyLink
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", AlertChangeLinkReasonOption.ChangeOfLink },
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
        var databaseLink = TestData.GenerateUrl();
        var journeyLink = TestData.GenerateUrl();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithExternalLink(databaseLink)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                AddLink = true,
                CurrentLink = databaseLink,
                Link = journeyLink
            });

        var reason = AlertChangeLinkReasonOption.AnotherReason;
        var HasAdditionalReasonDetail = true;
        var reasonDetail = "More details";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.StartsWith($"/alerts/{alertId}/link/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

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
        var databaseLink = TestData.GenerateUrl();
        var journeyLink = TestData.GenerateUrl();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithExternalLink(databaseLink)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                AddLink = true,
                CurrentLink = databaseLink,
                Link = journeyLink
            });

        var reason = AlertChangeLinkReasonOption.AnotherReason;
        var HasAdditionalReasonDetail = false;
        var evidenceFileName = "evidence.pdf";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/link/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.StartsWith($"/alerts/{alertId}/link/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

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
        var databaseLink = TestData.GenerateUrl();
        var journeyLink = TestData.GenerateUrl();
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithExternalLink(databaseLink)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            state: new()
            {
                Initialized = true,
                AddLink = true,
                CurrentLink = databaseLink,
                Link = journeyLink
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/link/change-reason/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

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

    private Task<JourneyInstance<EditAlertLinkState>> CreateJourneyInstance(Guid alertId, string? currentLink) =>
        CreateJourneyInstance(
            alertId,
            new EditAlertLinkState()
            {
                Initialized = true,
                CurrentLink = currentLink,
                Link = currentLink
            });

    private async Task<JourneyInstance<EditAlertLinkState>> CreateJourneyInstance(Guid alertId, EditAlertLinkState state) =>
        await CreateJourneyInstance(
            JourneyNames.EditAlertLink,
            state,
            new KeyValuePair<string, object>("alertId", alertId));
}
