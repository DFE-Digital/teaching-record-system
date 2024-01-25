using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.StartDate;

public class ReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_Redirects()
    {
        // Arrange        
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqStartDateState()
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_ReturnsOK()
    {
        // Arrange        
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqStartDateState()
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenNoChangeReasonIsSelected_ReturnsError()
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqStartDateState()
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "UploadEvidence", "False" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "ChangeReason", "Select a reason for change");
    }

    [Fact]
    public async Task Post_WhenNoUploadEvidenceOptionIsSelected_ReturnsError()
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqStartDateState()
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                 { "ChangeReason", MqChangeStartDateReasonOption.ChangeOfStartDate.ToString() },
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
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqStartDateState()
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                 { "ChangeReason", MqChangeStartDateReasonOption.ChangeOfStartDate.ToString() },
                 { "UploadEvidence", "True" }
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
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqStartDateState()
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate
            });

        var multipartContent = CreateFormFileUpload(".cs");
        multipartContent.Add(new StringContent(MqChangeStartDateReasonOption.ChangeOfStartDate.ToString()), "ChangeReason");
        multipartContent.Add(new StringContent("My change reason detail"), "ChangeReasonDetail");
        multipartContent.Add(new StringContent("True"), "UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Fact]
    public async Task Post_ValidInput_RedirectsToConfirmPage()
    {
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqStartDateState()
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate
            });

        var multipartContent = CreateFormFileUpload(".png");
        multipartContent.Add(new StringContent(MqChangeStartDateReasonOption.ChangeOfStartDate.ToString()), "ChangeReason");
        multipartContent.Add(new StringContent("My change reason detail"), "ChangeReasonDetail");
        multipartContent.Add(new StringContent("True"), "UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/start-date/confirm?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqStartDateState()
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/change-reason/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

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

    private async Task<JourneyInstance<EditMqStartDateState>> CreateJourneyInstance(Guid qualificationId, EditMqStartDateState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqStartDate,
            state ?? new EditMqStartDateState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
