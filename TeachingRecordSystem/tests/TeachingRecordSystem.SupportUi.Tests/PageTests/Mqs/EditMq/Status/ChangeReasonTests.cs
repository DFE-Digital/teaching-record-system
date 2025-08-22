using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Status;

public class ChangeReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/status/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_Redirects()
    {
        // Arrange        
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStatusState()
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/status/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/status?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_ReturnsOK()
    {
        // Arrange
        var oldStatus = MandatoryQualificationStatus.Failed;
        var newStatus = MandatoryQualificationStatus.Passed;
        var newEndDate = new DateOnly(2021, 12, 5);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStatusState()
            {
                Initialized = true,
                Status = newStatus,
                EndDate = newEndDate,
                CurrentStatus = oldStatus,
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/status/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Post_WhenNoChangeReasonIsSelected_ReturnsError(
        bool isStatusChange,
        bool isEndDateChange)
    {
        // Arrange
        MandatoryQualificationStatus oldStatus;
        MandatoryQualificationStatus newStatus;
        DateOnly? oldEndDate;
        DateOnly? newEndDate;
        string expectedErrorField = "";

        if (isStatusChange)
        {
            if (isEndDateChange)
            {
                oldStatus = MandatoryQualificationStatus.Failed;
                newStatus = MandatoryQualificationStatus.Passed;
                oldEndDate = null;
                newEndDate = new DateOnly(2021, 12, 5);
                expectedErrorField = "StatusChangeReason";
            }
            else
            {
                oldStatus = MandatoryQualificationStatus.InProgress;
                newStatus = MandatoryQualificationStatus.Failed;
                oldEndDate = null;
                newEndDate = null;
                expectedErrorField = "StatusChangeReason";
            }
        }
        else
        {
            oldStatus = MandatoryQualificationStatus.Passed;
            newStatus = MandatoryQualificationStatus.Passed;
            oldEndDate = new DateOnly(2021, 12, 5);
            newEndDate = new DateOnly(2021, 12, 6);
            expectedErrorField = "EndDateChangeReason";
        }

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus, oldEndDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStatusState()
            {
                Initialized = true,
                CurrentStatus = oldStatus,
                Status = newStatus,
                CurrentEndDate = oldEndDate,
                EndDate = newEndDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "UploadEvidence", "False" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, expectedErrorField, "Select a reason");
    }

    [Fact]
    public async Task Post_WhenNoUploadEvidenceOptionIsSelected_ReturnsError()
    {
        // Arrange
        var oldStatus = MandatoryQualificationStatus.Failed;
        var newStatus = MandatoryQualificationStatus.Passed;
        var newEndDate = new DateOnly(2021, 12, 5);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStatusState()
            {
                Initialized = true,
                Status = newStatus,
                EndDate = newEndDate,
                CurrentStatus = oldStatus,
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                 { "ChangeReason", MqChangeStatusReasonOption.ChangeOfStatus.ToString() },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "UploadEvidence", "Select yes if you want to upload evidence");
    }

    [Fact]
    public async Task Post_WhenUploadEvidenceOptionIsYesAndNoFileIsSelected_ReturnsError()
    {
        // Arrange
        var oldStatus = MandatoryQualificationStatus.Failed;
        var newStatus = MandatoryQualificationStatus.Passed;
        var newEndDate = new DateOnly(2021, 12, 5);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStatusState()
            {
                Initialized = true,
                Status = newStatus,
                EndDate = newEndDate,
                CurrentStatus = oldStatus,
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                 { "ChangeReason", MqChangeStatusReasonOption.ChangeOfStatus.ToString() },
                 { "UploadEvidence", "True" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EvidenceFile", "Select a file");
    }

    [Fact]
    public async Task Post_WhenEvidenceFileIsInvalidType_ReturnsError()
    {
        // Arrange
        var oldStatus = MandatoryQualificationStatus.Failed;
        var newStatus = MandatoryQualificationStatus.Passed;
        var newEndDate = new DateOnly(2021, 12, 5);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStatusState()
            {
                Initialized = true,
                Status = newStatus,
                EndDate = newEndDate,
                CurrentStatus = oldStatus,
            });

        var multipartContent = CreateFormFileUpload(".cs");
        multipartContent.Add(new StringContent(MqChangeStatusReasonOption.ChangeOfStatus.ToString()), "ChangeReason");
        multipartContent.Add(new StringContent("My change reason detail"), "ChangeReasonDetail");
        multipartContent.Add(new StringContent("True"), "UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Post_ValidInput_RedirectsToCheckAnswersPage(
        bool isStatusChange,
        bool isEndDateChange)
    {
        // Arrange
        MandatoryQualificationStatus oldStatus;
        MandatoryQualificationStatus newStatus;
        DateOnly? oldEndDate;
        DateOnly? newEndDate;
        string changeReasonKey = "";
        string changeReason = "";

        if (isStatusChange)
        {
            if (isEndDateChange)
            {
                oldStatus = MandatoryQualificationStatus.Failed;
                newStatus = MandatoryQualificationStatus.Passed;
                oldEndDate = null;
                newEndDate = new DateOnly(2021, 12, 5);
                changeReasonKey = "StatusChangeReason";
                changeReason = MqChangeStatusReasonOption.ChangeOfStatus.ToString();
            }
            else
            {
                oldStatus = MandatoryQualificationStatus.InProgress;
                newStatus = MandatoryQualificationStatus.Failed;
                oldEndDate = null;
                newEndDate = null;
                changeReasonKey = "StatusChangeReason";
                changeReason = MqChangeStatusReasonOption.ChangeOfStatus.ToString();
            }
        }
        else
        {
            oldStatus = MandatoryQualificationStatus.Passed;
            newStatus = MandatoryQualificationStatus.Passed;
            oldEndDate = new DateOnly(2021, 12, 5);
            newEndDate = new DateOnly(2021, 12, 6);
            changeReasonKey = "EndDateChangeReason";
            changeReason = MqChangeEndDateReasonOption.ChangeOfEndDate.ToString();
        }

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus, oldEndDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStatusState()
            {
                Initialized = true,
                CurrentStatus = oldStatus,
                Status = newStatus,
                CurrentEndDate = oldEndDate,
                EndDate = newEndDate
            });

        var multipartContent = CreateFormFileUpload(".png");
        multipartContent.Add(new StringContent(changeReason), changeReasonKey);
        multipartContent.Add(new StringContent("True"), "HasAdditionalReasonDetail");
        multipartContent.Add(new StringContent("My change reason detail"), "ChangeReasonDetail");
        multipartContent.Add(new StringContent("True"), "UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/status/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var oldStatus = MandatoryQualificationStatus.Failed;
        var newStatus = MandatoryQualificationStatus.Passed;
        var newEndDate = new DateOnly(2021, 12, 5);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStatusState()
            {
                Initialized = true,
                Status = newStatus,
                EndDate = newEndDate,
                CurrentStatus = oldStatus,
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/change-reason/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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

    [Theory]
    [MemberData(nameof(HttpMethods), TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var oldStatus = MandatoryQualificationStatus.Failed;
        var newStatus = MandatoryQualificationStatus.Passed;
        var newEndDate = new DateOnly(2021, 12, 5);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus)));
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStatusState()
            {
                Initialized = true,
                Status = newStatus,
                EndDate = newEndDate,
                CurrentStatus = oldStatus,
            });

        var request = new HttpRequestMessage(httpMethod, $"/mqs/{qualificationId}/status/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
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

    private async Task<JourneyInstance<EditMqStatusState>> CreateJourneyInstanceAsync(Guid qualificationId, EditMqStatusState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqStatus,
            state ?? new EditMqStatusState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
