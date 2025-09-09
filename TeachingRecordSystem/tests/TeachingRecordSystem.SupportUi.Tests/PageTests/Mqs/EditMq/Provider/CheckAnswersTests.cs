using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Provider;

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
            new EditMqProviderState()
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/provider/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithProvider(oldProvider.MandatoryQualificationProviderId)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var changeReason = MqChangeProviderReasonOption.ChangeOfTrainingProvider;
        var journeyInstance = await CreateJourneyInstanceAsync(
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/provider/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var changeSummary = doc.GetElementByTestId("change-summary");
        Assert.NotNull(changeSummary);
        Assert.Equal(oldProvider.Name, changeSummary.GetElementByTestId("current-provider")!.TrimmedText());
        Assert.Equal(newProvider.Name, changeSummary.GetElementByTestId("new-provider")!.TrimmedText());
        Assert.Equal(changeReason.GetDisplayName(), changeSummary.GetElementByTestId("change-reason")!.TrimmedText());
        Assert.Equal(!string.IsNullOrEmpty(changeReasonDetail) ? changeReasonDetail : "None", changeSummary.GetElementByTestId("change-reason-detail")!.TrimmedText());
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
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqProviderState()
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithProvider(oldProvider.MandatoryQualificationProviderId)));
        var qualification = person.MandatoryQualifications.First();
        var qualificationId = qualification.QualificationId;

        EventPublisher.Clear();

        var journeyInstance = await CreateJourneyInstanceAsync(
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
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
                        DqtMqEstablishmentName = null,
                        DqtMqEstablishmentValue = null
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
                        DqtMqEstablishmentName = null,
                        DqtMqEstablishmentValue = null
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
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithProvider(oldProvider.MandatoryQualificationProviderId)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/check-answers/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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

    [Theory]
    [MemberData(nameof(HttpMethods), TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var oldProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var newProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithProvider(oldProvider.MandatoryQualificationProviderId)));
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
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

        var request = new HttpRequestMessage(httpMethod, $"/mqs/{qualificationId}/provider/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private async Task<JourneyInstance<EditMqProviderState>> CreateJourneyInstanceAsync(Guid qualificationId, EditMqProviderState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqProvider,
            state ?? new EditMqProviderState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
