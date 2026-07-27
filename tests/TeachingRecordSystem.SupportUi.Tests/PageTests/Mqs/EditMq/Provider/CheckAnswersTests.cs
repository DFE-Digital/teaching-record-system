using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Provider;

public class CheckAnswersTests(HostFixture hostFixture) : EditMqProviderTestBase(hostFixture)
{
    [Theory]
    [InlineData(MqChangeProviderReasonOption.ChangeOfTrainingProvider, null, false, true, "Additional info")]
    [InlineData(MqChangeProviderReasonOption.AnotherReason, "Some reason", true, false, null)]
    public async Task Get_ValidRequest_DisplaysContentAsExpected(
        MqChangeProviderReasonOption changeReason,
        string? changeReasonDetail,
        bool uploadEvidence,
        bool provideAdditionalInformation,
        string? additionalInformation)
    {
        // Arrange
        var oldProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var newProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithProvider(oldProvider.MandatoryQualificationProviderId)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqProviderState
            {
                ProviderId = newProvider.MandatoryQualificationProviderId,
                CurrentProviderId = oldProvider.MandatoryQualificationProviderId,
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/provider/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var changeSummary = doc.GetElementByTestId("change-summary");
        Assert.NotNull(changeSummary);
        Assert.Equal(oldProvider.Name, changeSummary.GetElementByTestId("current-provider")!.TrimmedText());
        Assert.Equal(newProvider.Name, changeSummary.GetElementByTestId("new-provider")!.TrimmedText());

        var changeReasonSummary = doc.GetElementByTestId("change-reason-summary");
        Assert.NotNull(changeReasonSummary);
        Assert.Equal(changeReason.GetDisplayName(), changeReasonSummary.GetElementByTestId("change-reason")!.TrimmedText());
        Assert.Equal(!string.IsNullOrEmpty(changeReasonDetail) ? changeReasonDetail : "None", changeReasonSummary.GetElementByTestId("change-reason-detail")!.TrimmedText());
        var uploadedEvidenceLink = changeReasonSummary.GetElementByTestId("uploaded-evidence-file-link");
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
    public async Task Post_Confirm_UpdatesMqCreatesEventAndCompletesJourneyRedirectsWithFlashMessage()
    {
        // Arrange
        var oldProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var newProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithProvider(oldProvider.MandatoryQualificationProviderId)));
        var qualification = person.MandatoryQualifications.First();
        var qualificationId = qualification.QualificationId;

        EventObserver.Clear();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqProviderState
            {
                ProviderId = newProvider.MandatoryQualificationProviderId,
                CurrentProviderId = oldProvider.MandatoryQualificationProviderId,
                ChangeReason = MqChangeProviderReasonOption.AnotherReason,
                ChangeReasonDetail = "Some reason",
                Evidence = new()
                {
                    UploadEvidence = false

                },
                ProvideAdditionalInformation = true,
                AdditionalInformation = "some Additional reason"
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
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, "Mandatory qualification changed");

        Assert.Null(GetJourneyInstanceState(journeyInstance));

        await WithDbContextAsync(async dbContext =>
        {
            var qualification = await dbContext.MandatoryQualifications.SingleAsync(q => q.PersonId == person.PersonId);
            Assert.Equal(newProvider.MandatoryQualificationProviderId, qualification.ProviderId);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.MandatoryQualificationUpdating, p.ProcessContext.ProcessType);
            Assert.Equal(GetCurrentUserId(), p.ProcessContext.Process.UserId);
            p.AssertProcessHasEvent<MandatoryQualificationUpdatedEvent>(e =>
            {
                Assert.Equal(person.PersonId, e.PersonId);
                Assert.Equal(qualificationId, e.MandatoryQualification.QualificationId);
                Assert.Equal(MandatoryQualificationUpdatedEventChanges.Provider, e.Changes);
                Assert.Equal(newProvider.MandatoryQualificationProviderId, e.MandatoryQualification.Provider?.MandatoryQualificationProviderId);
                Assert.Equal(oldProvider.MandatoryQualificationProviderId, e.OldMandatoryQualification.Provider?.MandatoryQualificationProviderId);
            });

            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(p.ProcessContext.Process.ChangeReason);
            Assert.Equal(MqChangeProviderReasonOption.AnotherReason.GetDisplayName(), changeReason.Reason);
            Assert.Equal("Some reason", changeReason.Details);
            Assert.Equal("some Additional reason", changeReason.AdditionalInformation);
            Assert.Null(changeReason.EvidenceFile);
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
            new EditMqProviderState
            {
                ProviderId = newProvider.MandatoryQualificationProviderId,
                CurrentProviderId = oldProvider.MandatoryQualificationProviderId,
                ChangeReason = MqChangeProviderReasonOption.ChangeOfTrainingProvider,
                ChangeReasonDetail = null,
                Evidence = new()
                {
                    UploadEvidence = false
                },
                ProvideAdditionalInformation = false,
                AdditionalInformation = null,
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        Assert.Null(GetJourneyInstanceState(journeyInstance));

        await WithDbContextAsync(async dbContext =>
        {
            var qualification = await dbContext.MandatoryQualifications.SingleAsync(q => q.PersonId == person.PersonId);
            Assert.Equal(oldProvider.MandatoryQualificationProviderId, qualification.ProviderId);
        });
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var oldProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var newProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithProvider(oldProvider.MandatoryQualificationProviderId)));
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
                ProviderId = newProvider.MandatoryQualificationProviderId,
                CurrentProviderId = oldProvider.MandatoryQualificationProviderId,
                ChangeReason = MqChangeProviderReasonOption.ChangeOfTrainingProvider,
                ChangeReasonDetail = "Some reason",
                Evidence = new()
                {
                    UploadEvidence = false
                }
            });

        var request = new HttpRequestMessage(httpMethod, $"/mqs/{qualificationId}/provider/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

}
