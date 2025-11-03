using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.DeleteMq;

public class CheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_MissingDataInJourneyState_Redirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new DeleteMqState
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("University of Leeds", MandatoryQualificationSpecialism.Hearing, "2021-10-05", MandatoryQualificationStatus.Passed, "2021-11-05", MqDeletionReasonOption.ProviderRequest, "Some details about the deletion reason", true)]
    [InlineData("University of Leeds", MandatoryQualificationSpecialism.Hearing, "2021-10-05", MandatoryQualificationStatus.Deferred, null, MqDeletionReasonOption.ProviderRequest, null, false)]
    [InlineData(null, null, null, null, null, MqDeletionReasonOption.AnotherReason, null, false)]
    public async Task Get_ValidRequest_DisplaysContentAsExpected(
        string? providerName,
        MandatoryQualificationSpecialism? specialism,
        string? startDateString,
        MandatoryQualificationStatus? status,
        string? endDateString,
        MqDeletionReasonOption deletionReason,
        string? deletionReasonDetail,
        bool uploadEvidence)
    {
        // Arrange
        var provider = providerName is not null ? MandatoryQualificationProvider.All.Single(p => p.Name == providerName) : null;
        DateOnly? startDate = !string.IsNullOrEmpty(startDateString) ? DateOnly.Parse(startDateString) : null;
        DateOnly? endDate = !string.IsNullOrEmpty(endDateString) ? DateOnly.Parse(endDateString) : null;

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithProvider(provider?.MandatoryQualificationProviderId)
            .WithSpecialism(specialism)
            .WithStartDate(startDate)
            .WithStatus(status, endDate)));
        var qualification = person.MandatoryQualifications.Single();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualification.QualificationId,
            new DeleteMqState
            {
                Initialized = true,
                DeletionReason = deletionReason,
                DeletionReasonDetail = deletionReasonDetail,
                Evidence = new()
                {
                    UploadEvidence = uploadEvidence,
                    UploadedEvidenceFile = uploadEvidence ? new()
                    {
                        FileId = Guid.NewGuid(),
                        FileName = "test.pdf",
                        FileSizeDescription = "1MB"
                    } : null
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualification.QualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var deletionSummary = doc.GetElementByTestId("deletion-summary");
        Assert.NotNull(deletionSummary);
        Assert.Equal(deletionReason.GetDisplayName(), deletionSummary.GetElementByTestId("deletion-reason")!.TrimmedText());
        Assert.Equal(!string.IsNullOrEmpty(deletionReasonDetail) ? deletionReasonDetail : "None", deletionSummary.GetElementByTestId("deletion-reason-detail")!.TrimmedText());
        Assert.Equal(provider?.Name ?? "None", deletionSummary.GetElementByTestId("provider")!.TrimmedText());
        Assert.Equal(specialism?.GetTitle() ?? "None", deletionSummary.GetElementByTestId("specialism")!.TrimmedText());
        Assert.Equal(status is not null ? status.Value.ToString() : "None", deletionSummary.GetElementByTestId("status")!.TrimmedText());
        Assert.Equal(startDate is not null ? startDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : "None", deletionSummary.GetElementByTestId("start-date")!.TrimmedText());
        Assert.Equal(endDate is not null ? endDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : "None", deletionSummary.GetElementByTestId("end-date")!.TrimmedText());
        var uploadedEvidenceLink = doc.GetElementByTestId("uploaded-evidence-file-link");
        if (uploadEvidence)
        {
            Assert.NotNull(uploadedEvidenceLink);
        }
        else
        {
            Assert.Null(uploadedEvidenceLink);
        }
    }

    [Fact]
    public async Task Post_MissingDataInJourneyState_Redirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new DeleteMqState
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Confirm_DeletesMqCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var provider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var status = MandatoryQualificationStatus.Passed;
        var startDate = new DateOnly(2023, 09, 01);
        var endDate = new DateOnly(2023, 11, 05);
        var deletionReason = MqDeletionReasonOption.ProviderRequest;
        var deletionReasonDetail = "Some details about the deletion reason";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithProvider(provider.MandatoryQualificationProviderId)
            .WithSpecialism(specialism)
            .WithStartDate(startDate)
            .WithStatus(status, endDate)));

        EventObserver.Clear();

        var qualificationId = person.MandatoryQualifications!.Single().QualificationId;

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new DeleteMqState
            {
                Initialized = true,
                DeletionReason = deletionReason,
                DeletionReasonDetail = deletionReasonDetail,
                Evidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = evidenceFileId,
                        FileName = evidenceFileName,
                        FileSizeDescription = "1MB"
                    }
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Mandatory qualification deleted");

        EventObserver.AssertEventsSaved(e =>
        {
            var expectedMqDeletedEvent = new MandatoryQualificationDeletedEvent
            {
                EventId = Guid.Empty,
                CreatedUtc = Clock.UtcNow,
                RaisedBy = GetCurrentUserId(),
                PersonId = person.PersonId,
                MandatoryQualification = new()
                {
                    QualificationId = qualificationId,
                    Provider = new()
                    {
                        MandatoryQualificationProviderId = provider.MandatoryQualificationProviderId,
                        Name = provider.Name,
                        DqtMqEstablishmentName = null,
                        DqtMqEstablishmentValue = null
                    },
                    Specialism = specialism,
                    Status = status,
                    StartDate = startDate,
                    EndDate = endDate
                },
                DeletionReason = deletionReason.GetDisplayName(),
                DeletionReasonDetail = deletionReasonDetail,
                EvidenceFile = new EventModels.File
                {
                    FileId = evidenceFileId,
                    Name = evidenceFileName
                }
            };

            var actualMqDeletedEvent = Assert.IsType<MandatoryQualificationDeletedEvent>(e);
            Assert.Equivalent(expectedMqDeletedEvent with { EventId = actualMqDeletedEvent.EventId }, actualMqDeletedEvent);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new DeleteMqState
            {
                Initialized = true,
                DeletionReason = MqDeletionReasonOption.ProviderRequest,
                DeletionReasonDetail = "Some details about the deletion reason",
                Evidence = new()
                {
                    UploadEvidence = false
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/delete/check-answers/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var provider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var status = MandatoryQualificationStatus.Passed;
        var startDate = new DateOnly(2023, 09, 01);
        var endDate = new DateOnly(2023, 11, 05);
        var deletionReason = MqDeletionReasonOption.ProviderRequest;
        var deletionReasonDetail = "Some details about the deletion reason";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithProvider(provider.MandatoryQualificationProviderId)
            .WithSpecialism(specialism)
            .WithStartDate(startDate)
            .WithStatus(status, endDate)));
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var qualificationId = person.MandatoryQualifications!.Single().QualificationId;

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new DeleteMqState
            {
                Initialized = true,
                DeletionReason = deletionReason,
                DeletionReasonDetail = deletionReasonDetail,
                Evidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = evidenceFileId,
                        FileName = evidenceFileName,
                        FileSizeDescription = "1MB"
                    }
                }
            });

        var request = new HttpRequestMessage(httpMethod, $"/mqs/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private async Task<JourneyInstance<DeleteMqState>> CreateJourneyInstanceAsync(Guid qualificationId, DeleteMqState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.DeleteMq,
            state ?? new DeleteMqState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
