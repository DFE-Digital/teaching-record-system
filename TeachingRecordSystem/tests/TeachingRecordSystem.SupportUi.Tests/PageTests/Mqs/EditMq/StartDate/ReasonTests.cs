using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.StartDate;

public class ReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_MissingDataInJourneyState_Redirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_ReturnsOK()
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_WhenNoChangeReasonIsSelected_ReturnsError()
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Evidence.UploadEvidence", "False" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ChangeReason", "Select a reason");
    }

    [Test]
    public async Task Post_WhenNoUploadEvidenceOptionIsSelected_ReturnsError()
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                 { "ChangeReason", MqChangeStartDateReasonOption.ChangeOfStartDate.ToString() },
                 { "Evidence.UploadEvidence", "" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.UploadEvidence", "Select yes if you want to upload evidence");
    }

    [Test]
    public async Task Post_WhenUploadEvidenceOptionIsYesAndNoFileIsSelected_ReturnsError()
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                 { "ChangeReason", MqChangeStartDateReasonOption.ChangeOfStartDate.ToString() },
                 { "Evidence.UploadEvidence", "True" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.EvidenceFile", "Select a file");
    }

    [Test]
    public async Task Post_WhenEvidenceFileIsInvalidType_ReturnsError()
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate
            });

        var multipartContent = CreateFormFileUpload(".cs");
        multipartContent.Add(new StringContent(MqChangeStartDateReasonOption.ChangeOfStartDate.ToString()), "ChangeReason");
        multipartContent.Add(new StringContent("My change reason detail"), "ChangeReasonDetail");
        multipartContent.Add(new StringContent("True"), "Evidence.UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Test]
    public async Task Post_ValidInput_RedirectsToCheckAnswersPage()
    {
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate
            });

        var multipartContent = CreateFormFileUpload(".png");
        multipartContent.Add(new StringContent(MqChangeStartDateReasonOption.ChangeOfStartDate.ToString()), "ChangeReason");
        multipartContent.Add(new StringContent("True"), "HasAdditionalReasonDetail");
        multipartContent.Add(new StringContent("My change reason detail"), "ChangeReasonDetail");
        multipartContent.Add(new StringContent("True"), "Evidence.UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/reason/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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

    [Test]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate
            });

        var request = new HttpRequestMessage(httpMethod, $"/mqs/{qualificationId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private MultipartFormDataContent CreateFormFileUpload(string fileExtension)
    {
        var byteArrayContent = new ByteArrayContent([]);
        byteArrayContent.Headers.Add("Content-Type", "application/octet-stream");

        var multipartContent = new MultipartFormDataContent
        {
            { byteArrayContent, "Evidence.EvidenceFile", $"evidence{fileExtension}" }
        };

        return multipartContent;
    }

    private async Task<JourneyInstance<EditMqStartDateState>> CreateJourneyInstanceAsync(Guid qualificationId, EditMqStartDateState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqStartDate,
            state ?? new EditMqStartDateState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
