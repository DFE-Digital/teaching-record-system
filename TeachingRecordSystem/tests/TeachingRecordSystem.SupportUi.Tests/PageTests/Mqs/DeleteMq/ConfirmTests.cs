using FormFlow;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.DeleteMq;

public class ConfirmTests : TestBase
{
    public ConfirmTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_Redirects()
    {
        // Arrange        
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new DeleteMqState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false)
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("959", MandatoryQualificationSpecialism.Hearing, "2021-10-05", MandatoryQualificationStatus.Passed, "2021-11-05", MqDeletionReasonOption.ProviderRequest, "Some details about the deletion reason", true)]
    [InlineData("959", MandatoryQualificationSpecialism.Hearing, "2021-10-05", MandatoryQualificationStatus.Deferred, null, MqDeletionReasonOption.ProviderRequest, null, false)]
    [InlineData(null, null, null, null, null, MqDeletionReasonOption.AnotherReason, null, false)]
    public async Task Get_ValidRequest_DisplaysContentAsExpected(
        string? providerValue,
        MandatoryQualificationSpecialism? specialism,
        string? startDateString,
        MandatoryQualificationStatus? status,
        string? endDateString,
        MqDeletionReasonOption deletionReason,
        string? deletionReasonDetail,
        bool uploadEvidence)
    {
        // Arrange
        var mqEstablishment = !string.IsNullOrEmpty(providerValue) ? await TestData.ReferenceDataCache.GetMqEstablishmentByValue(providerValue) : null;
        DateOnly? startDate = !string.IsNullOrEmpty(startDateString) ? DateOnly.Parse(startDateString) : null;
        DateOnly? endDate = !string.IsNullOrEmpty(endDateString) ? DateOnly.Parse(endDateString) : null;

        var person = await TestData.CreatePerson(
            b => b.WithMandatoryQualification(
                providerValue: providerValue,
                specialism: specialism,
                startDate: startDate,
                endDate: endDate,
                status: status));
        var qualification = person.MandatoryQualifications.Single();
        var journeyInstance = await CreateJourneyInstance(
            qualification.QualificationId,
            new DeleteMqState
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                ProviderName = mqEstablishment?.dfeta_name,
                Specialism = specialism,
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
                DeletionReason = deletionReason,
                DeletionReasonDetail = deletionReasonDetail,
                UploadEvidence = uploadEvidence,
                EvidenceFileId = uploadEvidence ? Guid.NewGuid() : null,
                EvidenceFileName = uploadEvidence ? "test.pdf" : null,
                EvidenceFileSizeDescription = uploadEvidence ? "1MB" : null
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualification.QualificationId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var deletionSummary = doc.GetElementByTestId("deletion-summary");
        Assert.NotNull(deletionSummary);
        Assert.Equal(deletionReason.GetDisplayName(), deletionSummary.GetElementByTestId("deletion-reason")!.TextContent);
        Assert.Equal(!string.IsNullOrEmpty(deletionReasonDetail) ? deletionReasonDetail : "None", deletionSummary.GetElementByTestId("deletion-reason-detail")!.TextContent);
        Assert.Equal(mqEstablishment is not null ? mqEstablishment.dfeta_name : "None", deletionSummary.GetElementByTestId("provider")!.TextContent);
        Assert.Equal(specialism?.GetTitle() ?? "None", deletionSummary.GetElementByTestId("specialism")!.TextContent);
        Assert.Equal(status is not null ? status.Value.ToString() : "None", deletionSummary.GetElementByTestId("status")!.TextContent);
        Assert.Equal(startDate is not null ? startDate.Value.ToString("d MMMM yyyy") : "None", deletionSummary.GetElementByTestId("start-date")!.TextContent);
        Assert.Equal(endDate is not null ? endDate.Value.ToString("d MMMM yyyy") : "None", deletionSummary.GetElementByTestId("end-date")!.TextContent);
        var uploadedEvidenceLink = doc.GetElementByTestId("uploaded-evidence-link");
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
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new DeleteMqState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false)
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        var mqEstablishmentDqtValue = "959";  // "University of Leeds"
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var status = MandatoryQualificationStatus.Passed;
        var startDate = new DateOnly(2023, 09, 01);
        var endDate = new DateOnly(2023, 11, 05);
        var deletionReason = MqDeletionReasonOption.ProviderRequest;
        var deletionReasonDetail = "Some details about the deletion reason";
        var evidenceFileId = Guid.NewGuid();
        var evidenceFileName = "test.pdf";

        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(
            providerValue: mqEstablishmentDqtValue,
            specialism: specialism,
            status: status,
            startDate: startDate,
            endDate: endDate));

        var qualificationId = person.MandatoryQualifications!.Single().QualificationId;
        var mqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue(mqEstablishmentDqtValue);
        MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(mqEstablishment, out var provider);
        Assert.NotNull(provider);

        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new DeleteMqState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                ProviderName = mqEstablishment.dfeta_name,
                Specialism = specialism,
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
                DeletionReason = deletionReason,
                DeletionReasonDetail = deletionReasonDetail,
                UploadEvidence = true,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName,
                EvidenceFileSizeDescription = "1MB"
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Mandatory qualification deleted");

        await WithDbContext(async dbContext =>
        {
            var mq = await dbContext.MandatoryQualifications.IgnoreQueryFilters().SingleOrDefaultAsync(mq => mq.QualificationId == qualificationId);
            Assert.NotNull(mq);
            Assert.Equal(Clock.UtcNow, mq.DeletedOn);
        });

        EventObserver.AssertEventsSaved(e =>
        {
            var expectedMqDeletedEvent = new MandatoryQualificationDeletedEvent()
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
                        DqtMqEstablishmentId = mqEstablishment.Id,
                        DqtMqEstablishmentName = mqEstablishment.dfeta_name
                    },
                    Specialism = specialism,
                    Status = status,
                    StartDate = startDate,
                    EndDate = endDate
                },
                DeletionReason = deletionReason.GetDisplayName(),
                DeletionReasonDetail = deletionReasonDetail,
                EvidenceFile = new Core.Events.Models.File()
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
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new DeleteMqState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                ProviderName = "University of Leeds",
                Specialism = MandatoryQualificationSpecialism.Hearing,
                Status = MandatoryQualificationStatus.Passed,
                StartDate = new DateOnly(2023, 09, 01),
                EndDate = new DateOnly(2023, 11, 05),
                DeletionReason = MqDeletionReasonOption.ProviderRequest,
                DeletionReasonDetail = "Some details about the deletion reason",
                UploadEvidence = false
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/delete/confirm/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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

    private async Task<JourneyInstance<DeleteMqState>> CreateJourneyInstance(Guid qualificationId, DeleteMqState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.DeleteMq,
            state ?? new DeleteMqState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
