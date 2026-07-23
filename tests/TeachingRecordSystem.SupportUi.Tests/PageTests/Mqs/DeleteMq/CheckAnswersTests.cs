using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.DeleteMq;

public class CheckAnswersTests(HostFixture hostFixture) : DeleteMqTestBase(hostFixture)
{
    [Theory]
    [InlineData("University of Leeds", MandatoryQualificationSpecialism.Hearing, "2021-10-05", MandatoryQualificationStatus.Passed, "2021-11-05", MqDeletionReasonOption.AnotherReason, "Some details about the deletion reason", true, "additional info", true)]
    [InlineData("University of Leeds", MandatoryQualificationSpecialism.Hearing, "2021-10-05", MandatoryQualificationStatus.Deferred, null, MqDeletionReasonOption.ProviderRequest, null, false, "additional info", true)]
    [InlineData(null, null, null, null, null, MqDeletionReasonOption.AddedInError, null, false, "additional info", true)]
    [InlineData(null, null, null, null, null, MqDeletionReasonOption.AddedInError, null, false, null, false)]
    public async Task Get_ValidRequest_DisplaysContentAsExpected(
        string? providerName,
        MandatoryQualificationSpecialism? specialism,
        string? startDateString,
        MandatoryQualificationStatus? status,
        string? endDateString,
        MqDeletionReasonOption deletionReason,
        string? deletionReasonDetail,
        bool uploadEvidence,
        string? additionalInformation,
        bool provideAdditionalInformation)
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
                },
                ProvideAdditionalInformation = provideAdditionalInformation,
                AdditionalInformation = additionalInformation
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualification.QualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var deletionSummary = doc.GetElementByTestId("deletion-summary");
        Assert.NotNull(deletionSummary);
        Assert.Equal(provider?.Name ?? "None", deletionSummary.GetElementByTestId("provider")!.TrimmedText());
        Assert.Equal(specialism?.GetTitle() ?? "None", deletionSummary.GetElementByTestId("specialism")!.TrimmedText());
        Assert.Equal(status is not null ? status.Value.ToString() : "None", deletionSummary.GetElementByTestId("status")!.TrimmedText());
        Assert.Equal(startDate is not null ? startDate.Value.ToString(WebConstants.DateDisplayFormat) : "None", deletionSummary.GetElementByTestId("start-date")!.TrimmedText());
        Assert.Equal(endDate is not null ? endDate.Value.ToString(WebConstants.DateDisplayFormat) : "None", deletionSummary.GetElementByTestId("end-date")!.TrimmedText());

        var deletionReasonSummary = doc.GetElementByTestId("change-reason-summary");
        Assert.NotNull(deletionReasonSummary);
        Assert.Equal(deletionReason.GetDisplayName(), deletionReasonSummary.GetElementByTestId("deletion-reason")!.TrimmedText());
        Assert.Equal(!string.IsNullOrEmpty(deletionReasonDetail) ? deletionReasonDetail : "None", deletionReasonSummary.GetElementByTestId("deletion-reason-detail")!.TrimmedText());
        Assert.Equal(provideAdditionalInformation ? additionalInformation : "None", deletionReasonSummary.GetElementByTestId("additional-information")!.TrimmedText());
        var uploadedEvidenceLink = deletionReasonSummary.GetElementByTestId("uploaded-evidence-file-link");
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
    public async Task Post_Confirm_DeletesMqCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var provider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var status = MandatoryQualificationStatus.Passed;
        var startDate = new DateOnly(2023, 09, 01);
        var endDate = new DateOnly(2023, 11, 05);
        var deletionReason = MqDeletionReasonOption.AnotherReason;
        var deletionReasonDetail = "Some details about the deletion reason";
        var additionalInformation = "this is some additional info";
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
                },
                AdditionalInformation = additionalInformation,
                ProvideAdditionalInformation = true
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
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, "Mandatory qualification deleted");

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.MandatoryQualificationDeleting, p.ProcessContext.ProcessType);
            Assert.Equal(GetCurrentUserId(), p.ProcessContext.Process.UserId);
            p.AssertProcessHasEvent<MandatoryQualificationDeletedEvent>(e =>
            {
                Assert.Equal(person.PersonId, e.PersonId);
                Assert.Equal(qualificationId, e.MandatoryQualification.QualificationId);
                Assert.Equal(provider.MandatoryQualificationProviderId, e.MandatoryQualification.Provider?.MandatoryQualificationProviderId);
                Assert.Equal(provider.Name, e.MandatoryQualification.Provider?.Name);
                Assert.Equal(specialism, e.MandatoryQualification.Specialism);
                Assert.Equal(status, e.MandatoryQualification.Status);
                Assert.Equal(startDate, e.MandatoryQualification.StartDate);
                Assert.Equal(endDate, e.MandatoryQualification.EndDate);
            });

            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(p.ProcessContext.Process.ChangeReason);
            Assert.Equal(deletionReason.GetDisplayName(), changeReason.Reason);
            Assert.Equal(deletionReasonDetail, changeReason.Details);
            Assert.Equal(additionalInformation, changeReason.AdditionalInformation);
            Assert.Equal(new EventModels.File { FileId = evidenceFileId, Name = evidenceFileName }, changeReason.EvidenceFile);
        });

        Assert.Null(GetJourneyInstanceState(journeyInstance));
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
                DeletionReason = MqDeletionReasonOption.AnotherReason,
                DeletionReasonDetail = "Some details about the deletion reason",
                Evidence = new()
                {
                    UploadEvidence = false
                },
                ProvideAdditionalInformation = false,
                AdditionalInformation = null
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
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
                DeletionReason = deletionReason,
                DeletionReasonDetail = null,
                Evidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = evidenceFileId,
                        FileName = evidenceFileName,
                        FileSizeDescription = "1MB"
                    }
                },
                ProvideAdditionalInformation = false,
                AdditionalInformation = null
            });

        var request = new HttpRequestMessage(httpMethod, $"/mqs/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

}
