using FormFlow;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Provider;

public class ConfirmTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_MissingDataInJourneyState_Redirects()
    {
        // Arrange        
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqProviderState()
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/provider/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/provider?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("Some reason", true)]
    public async Task Get_ValidRequest_DisplaysContentAsExpected(
        string? changeReasonDetail,
        bool uploadEvidence)
    {
        // Arrange
        var oldMqEstablishmentValue = "955"; // University of Birmingham
        var oldMqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue(oldMqEstablishmentValue);
        var newMqEstablishmentValue = "959"; // University of Leeds
        var newMqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue(newMqEstablishmentValue);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithDqtMqEstablishmentValue(oldMqEstablishmentValue)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var changeReason = MqChangeProviderReasonOption.ChangeOfTrainingProvider;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqProviderState()
            {
                Initialized = true,
                MqEstablishmentValue = newMqEstablishmentValue,
                CurrentMqEstablishmentName = oldMqEstablishment.dfeta_name,
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = uploadEvidence,
                EvidenceFileId = uploadEvidence ? Guid.NewGuid() : null,
                EvidenceFileName = uploadEvidence ? "test.pdf" : null,
                EvidenceFileSizeDescription = uploadEvidence ? "1MB" : null
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/provider/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var changeSummary = doc.GetElementByTestId("change-summary");
        Assert.NotNull(changeSummary);
        Assert.Equal(oldMqEstablishment.dfeta_name, changeSummary.GetElementByTestId("current-provider")!.TextContent);
        Assert.Equal(newMqEstablishment.dfeta_name, changeSummary.GetElementByTestId("new-provider")!.TextContent);
        Assert.Equal(changeReason.GetDisplayName(), changeSummary.GetElementByTestId("change-reason")!.TextContent);
        Assert.Equal(!string.IsNullOrEmpty(changeReasonDetail) ? changeReasonDetail : "None", changeSummary.GetElementByTestId("change-reason-detail")!.TextContent);
        var uploadedEvidenceLink = changeSummary.GetElementByTestId("uploaded-evidence-link");
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
            new EditMqProviderState()
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/provider?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Confirm_UpdatesMqCreatesEventAndCompletesJourneyRedirectsWithFlashMessage()
    {
        // Arrange
        var oldMqEstablishmentValue = "955"; // University of Birmingham
        var newMqEstablishmentValue = "959"; // University of Leeds
        var oldEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue(oldMqEstablishmentValue);
        var newEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue(newMqEstablishmentValue);

        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithDqtMqEstablishmentValue(oldMqEstablishmentValue)));
        var qualification = person.MandatoryQualifications.First();
        var qualificationId = qualification.QualificationId;

        EventObserver.Clear();

        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqProviderState()
            {
                Initialized = true,
                MqEstablishmentValue = newMqEstablishmentValue,
                CurrentMqEstablishmentName = oldEstablishment.dfeta_name,
                ChangeReason = MqChangeProviderReasonOption.ChangeOfTrainingProvider,
                ChangeReasonDetail = "Some reason",
                UploadEvidence = false,
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Mandatory qualification changed");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);

        await WithDbContext(async dbContext =>
        {
            var qualification = await dbContext.MandatoryQualifications.SingleAsync(q => q.PersonId == person.PersonId);
            Assert.Equal(newEstablishment?.dfeta_mqestablishmentId, qualification.DqtMqEstablishmentId);
        });

        EventObserver.AssertEventsSaved(e =>
        {
            var expectedMqUpdatedEvent = new MandatoryQualificationUpdatedEvent()
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
                        MandatoryQualificationProviderId = null,
                        Name = null,
                        DqtMqEstablishmentId = newEstablishment.Id,
                        DqtMqEstablishmentName = newEstablishment.dfeta_name
                    },
                    Specialism = qualification.Specialism,
                    Status = qualification.Status,
                    StartDate = qualification.StartDate,
                    EndDate = qualification.EndDate
                },
                OldMandatoryQualification = new()
                {
                    QualificationId = qualificationId,
                    Provider = new()
                    {
                        MandatoryQualificationProviderId = null,
                        Name = null,
                        DqtMqEstablishmentId = oldEstablishment.Id,
                        DqtMqEstablishmentName = oldEstablishment.dfeta_name
                    },
                    Specialism = qualification.Specialism,
                    Status = qualification.Status,
                    StartDate = qualification.StartDate,
                    EndDate = qualification.EndDate
                },
                ChangeReason = MqChangeProviderReasonOption.ChangeOfTrainingProvider.GetDisplayName(),
                ChangeReasonDetail = "Some reason",
                EvidenceFile = null,
                Changes = MandatoryQualificationUpdatedEventChanges.Provider
            };

            var actualMqUpdatedEvent = Assert.IsType<MandatoryQualificationUpdatedEvent>(e);
            Assert.Equivalent(expectedMqUpdatedEvent with { EventId = actualMqUpdatedEvent.EventId }, actualMqUpdatedEvent);
        });
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsAndDoesNotUpdateMq()
    {
        // Arrange
        var oldMqEstablishmentValue = "955"; // University of Birmingham
        var newMqEstablishmentValue = "959"; // University of Leeds
        var oldEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue(oldMqEstablishmentValue);

        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithDqtMqEstablishmentValue(oldMqEstablishmentValue)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqProviderState()
            {
                Initialized = true,
                MqEstablishmentValue = newMqEstablishmentValue,
                ChangeReason = MqChangeProviderReasonOption.ChangeOfTrainingProvider,
                ChangeReasonDetail = "Some reason",
                UploadEvidence = false,
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/confirm/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);

        await WithDbContext(async dbContext =>
        {
            var qualification = await dbContext.MandatoryQualifications.SingleAsync(q => q.PersonId == person.PersonId);
            MandatoryQualificationProvider.TryMapFromDqtMqEstablishmentValue(oldMqEstablishmentValue, out var expectedProvider);
            Assert.Equal(oldEstablishment?.dfeta_mqestablishmentId, qualification.DqtMqEstablishmentId);
        });
    }

    private async Task<JourneyInstance<EditMqProviderState>> CreateJourneyInstance(Guid qualificationId, EditMqProviderState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqProvider,
            state ?? new EditMqProviderState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
