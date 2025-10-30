using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Provider;

public class ReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/provider/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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
            new EditMqProviderState
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/provider/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/provider?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_ReturnsOK()
    {
        // Arrange
        var databaseProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var journeyProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithProvider(databaseProvider.MandatoryQualificationProviderId)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqProviderState
            {
                Initialized = true,
                ProviderId = journeyProvider.MandatoryQualificationProviderId
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/provider/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_WhenNoChangeReasonIsSelected_ReturnsError()
    {
        // Arrange
        var databaseProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var journeyProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithProvider(databaseProvider.MandatoryQualificationProviderId)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqProviderState
            {
                Initialized = true,
                ProviderId = journeyProvider.MandatoryQualificationProviderId
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "UploadEvidence", "False" }
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
        var databaseProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var journeyProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithProvider(databaseProvider.MandatoryQualificationProviderId)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqProviderState
            {
                Initialized = true,
                ProviderId = journeyProvider.MandatoryQualificationProviderId
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                 { "ChangeReason", MqChangeProviderReasonOption.ChangeOfTrainingProvider.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "UploadEvidence", "Select yes if you want to upload evidence");
    }

    [Test]
    public async Task Post_WhenUploadEvidenceOptionIsYesAndNoFileIsSelected_ReturnsError()
    {
        // Arrange
        var databaseProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var journeyProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithProvider(databaseProvider.MandatoryQualificationProviderId)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqProviderState
            {
                Initialized = true,
                ProviderId = journeyProvider.MandatoryQualificationProviderId
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                 { "ChangeReason", MqChangeProviderReasonOption.ChangeOfTrainingProvider.ToString() },
                 { "UploadEvidence", "True" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EvidenceFile", "Select a file");
    }

    [Test]
    public async Task Post_WhenEvidenceFileIsInvalidType_ReturnsError()
    {
        // Arrange
        var databaseProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var journeyProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithProvider(databaseProvider.MandatoryQualificationProviderId)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqProviderState
            {
                Initialized = true,
                ProviderId = journeyProvider.MandatoryQualificationProviderId
            });

        var multipartContent = CreateFormFileUpload(".cs");
        multipartContent.Add(new StringContent(MqChangeProviderReasonOption.ChangeOfTrainingProvider.ToString()), "ChangeReason");
        multipartContent.Add(new StringContent("My change reason detail"), "ChangeReasonDetail");
        multipartContent.Add(new StringContent("True"), "UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Test]
    public async Task Post_ValidInput_RedirectsToCheckAnswersPage()
    {
        // Arrange
        var databaseProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var journeyProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithProvider(databaseProvider.MandatoryQualificationProviderId)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqProviderState
            {
                Initialized = true,
                ProviderId = journeyProvider.MandatoryQualificationProviderId
            });

        var multipartContent = CreateFormFileUpload(".png");
        multipartContent.Add(new StringContent(MqChangeProviderReasonOption.ChangeOfTrainingProvider.ToString()), "ChangeReason");
        multipartContent.Add(new StringContent("True"), "HasAdditionalReasonDetail");
        multipartContent.Add(new StringContent("My change reason detail"), "ChangeReasonDetail");
        multipartContent.Add(new StringContent("True"), "UploadEvidence");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = multipartContent
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/provider/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        var databaseProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var journeyProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithProvider(databaseProvider.MandatoryQualificationProviderId)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqProviderState
            {
                Initialized = true,
                ProviderId = journeyProvider.MandatoryQualificationProviderId
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/reason/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        var databaseProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var journeyProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithProvider(databaseProvider.MandatoryQualificationProviderId)));
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqProviderState
            {
                Initialized = true,
                ProviderId = journeyProvider.MandatoryQualificationProviderId
            });

        var request = new HttpRequestMessage(httpMethod, $"/mqs/{qualificationId}/provider/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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
            { byteArrayContent, "EvidenceFile", $"evidence{fileExtension}" }
        };

        return multipartContent;
    }

    private async Task<JourneyInstance<EditMqProviderState>> CreateJourneyInstanceAsync(Guid qualificationId, EditMqProviderState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqProvider,
            state ?? new EditMqProviderState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
