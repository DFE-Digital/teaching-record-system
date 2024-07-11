using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Provider;

public class ConfirmTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_MissingDataInJourneyState_Redirects()
    {
        // Arrange        
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
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
        var oldProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var newProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithProvider(oldProvider.MandatoryQualificationProviderId)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var changeReason = MqChangeProviderReasonOption.ChangeOfTrainingProvider;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqProviderState()
            {
                Initialized = true,
                ProviderId = newProvider.MandatoryQualificationProviderId,
                CurrentProviderId = oldProvider.MandatoryQualificationProviderId,
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
        var doc = await AssertEx.HtmlResponse(response);
        var changeSummary = doc.GetElementByTestId("change-summary");
        Assert.NotNull(changeSummary);
        Assert.Equal(oldProvider.Name, changeSummary.GetElementByTestId("current-provider")!.TextContent);
        Assert.Equal(newProvider.Name, changeSummary.GetElementByTestId("new-provider")!.TextContent);
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
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
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
        var oldProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var newProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithProvider(oldProvider.MandatoryQualificationProviderId)));
        var qualification = person.MandatoryQualifications.First();
        var qualificationId = qualification.QualificationId;

        EventPublisher.Clear();

        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqProviderState()
            {
                Initialized = true,
                ProviderId = newProvider.MandatoryQualificationProviderId,
                CurrentProviderId = oldProvider.MandatoryQualificationProviderId,
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
            Assert.Equal(newProvider.MandatoryQualificationProviderId, qualification.ProviderId);
        });

        EventPublisher.AssertEventsSaved(e =>
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
                        MandatoryQualificationProviderId = newProvider.MandatoryQualificationProviderId,
                        Name = newProvider.Name,
                        DqtMqEstablishmentId = null,
                        DqtMqEstablishmentName = null
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
                        MandatoryQualificationProviderId = oldProvider.MandatoryQualificationProviderId,
                        Name = oldProvider.Name,
                        DqtMqEstablishmentId = null,
                        DqtMqEstablishmentName = null
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
        var oldProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var newProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithProvider(oldProvider.MandatoryQualificationProviderId)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqProviderState()
            {
                Initialized = true,
                ProviderId = newProvider.MandatoryQualificationProviderId,
                CurrentProviderId = oldProvider.MandatoryQualificationProviderId,
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
            Assert.Equal(oldProvider.MandatoryQualificationProviderId, qualification.ProviderId);
        });
    }

    private async Task<JourneyInstance<EditMqProviderState>> CreateJourneyInstance(Guid qualificationId, EditMqProviderState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqProvider,
            state ?? new EditMqProviderState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
