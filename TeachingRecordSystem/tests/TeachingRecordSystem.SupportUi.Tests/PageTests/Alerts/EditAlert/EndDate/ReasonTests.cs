using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.EndDate;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.EndDate;

public class ReasonTests : TestBase
{
    public ReasonTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.AllAlertsWriter);
    }

    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                EndDate = journeyEndDate
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/end-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/end-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/end-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/end-date?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsOK()
    {
        // Arrange
        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                EndDate = journeyEndDate
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/end-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithReasonPopulatedDataInJourneyState_ReturnsExpectedContent()
    {
        // Arrange
        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var reason = AlertChangeEndDateReasonOption.AnotherReason;
        var reasonDetail = "My Reason";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileSizeDescription = "1 MB";

        var journeyInstance = await CreateJourneyInstance(alertId, state: new()
        {
            Initialized = true,
            CurrentEndDate = databaseEndDate,
            EndDate = journeyEndDate,
            ChangeReason = reason,
            HasAdditionalReasonDetail = true,
            ChangeReasonDetail = reasonDetail,
            UploadEvidence = true,
            EvidenceFileId = evidenceFileId,
            EvidenceFileName = evidenceFileName,
            EvidenceFileSizeDescription = evidenceFileSizeDescription,
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/end-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(alertId, state: new()
        {
            Initialized = true,
            CurrentEndDate = databaseEndDate,
            EndDate = journeyEndDate
        });

        var reason = AlertChangeStartDateReasonOption.AnotherReason;
        var HasAdditionalReasonDetail = true;
        var reasonDetail = "More details";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}/end-date?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenNoChangeReasonIsSelected_ReturnsError()
    {
        // Arrange
        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                EndDate = journeyEndDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["HasAdditionalReasonDetail"] = bool.FalseString,
                ["UploadEvidence"] = "False"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "ChangeReason", "Select a reason");
    }

    [Fact]
    public async Task Post_WhenNoHasAdditionalReasonDetailIsSelected_ReturnsError()
    {
        // Arrange
        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                CurrentEndDate = databaseEndDate,
                EndDate = journeyEndDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", AlertChangeEndDateReasonOption.IncorrectEndDate },
                { "UploadEvidence", bool.FalseString }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "HasAdditionalReasonDetail", "Select yes if you want to add more information about why you’re changing the end date");
    }

    [Fact]
    public async Task Post_WhenAdditionalDetailIsYesButAdditionalDetailsAreEmpty_ReturnsError()
    {
        // Arrange
        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                CurrentEndDate = databaseEndDate,
                EndDate = journeyEndDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", AlertChangeEndDateReasonOption.IncorrectEndDate },
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
        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                CurrentEndDate = databaseEndDate,
                EndDate = journeyEndDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", AlertChangeEndDateReasonOption.IncorrectEndDate },
                { "HasAdditionalReasonDetail", bool.TrueString },
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
        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                CurrentEndDate = databaseEndDate,
                EndDate = journeyEndDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
            {
                { "ChangeReason", AlertChangeEndDateReasonOption.IncorrectEndDate },
                { "HasAdditionalReasonDetail", bool.TrueString },
                { "UploadEvidence", bool.TrueString },
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
        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                CurrentEndDate = databaseEndDate,
                EndDate = journeyEndDate
            });

        var reason = AlertChangeEndDateReasonOption.ChangeOfEndDate;
        var HasAdditionalReasonDetail = true;
        var reasonDetail = "More details";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.StartsWith($"/alerts/{alertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

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
        var startDate = TestData.Clock.Today.AddDays(-50);
        var databaseEndDate = TestData.Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                CurrentEndDate = databaseEndDate,
                EndDate = journeyEndDate
            });

        var reason = AlertChangeEndDateReasonOption.ChangeOfEndDate;
        var HasAdditionalReasonDetail = false;
        var evidenceFileName = "evidence.pdf";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.StartsWith($"/alerts/{alertId}/end-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

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
        var startDate = Clock.Today.AddDays(-50);
        var databaseEndDate = Clock.Today.AddDays(-10);
        var journeyEndDate = TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(b => b.WithAlert(q => q.WithStartDate(startDate).WithEndDate(databaseEndDate)));
        var alertId = person.Alerts.Single().AlertId;
        var journeyInstance = await CreateJourneyInstance(
            alertId,
            new EditAlertEndDateState()
            {
                Initialized = true,
                CurrentEndDate = databaseEndDate,
                EndDate = journeyEndDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/end-date/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alertId}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private static HttpContent CreateEvidenceFileBinaryContent()
    {
        var byteArrayContent = new ByteArrayContent([]);
        byteArrayContent.Headers.Add("Content-Type", "application/octet-stream");
        return byteArrayContent;
    }

    private async Task<JourneyInstance<EditAlertEndDateState>> CreateJourneyInstance(Guid alertId, EditAlertEndDateState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditAlertEndDate,
            state ?? new EditAlertEndDateState(),
            new KeyValuePair<string, object>("alertId", alertId));
}
