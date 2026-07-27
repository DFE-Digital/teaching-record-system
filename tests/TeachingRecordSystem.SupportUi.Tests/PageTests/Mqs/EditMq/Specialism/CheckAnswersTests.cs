using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Specialism;

public class CheckAnswersTests(HostFixture hostFixture) : EditMqSpecialismTestBase(hostFixture)
{
    [Theory]
    [InlineData(MqChangeSpecialismReasonOption.ChangeOfSpecialism, null, false)]
    [InlineData(MqChangeSpecialismReasonOption.AnotherReason, "Some reason", true)]
    public async Task Get_ValidRequest_DisplaysContentAsExpected(
        MqChangeSpecialismReasonOption changeReason,
        string? changeReasonDetail,
        bool uploadEvidence)
    {
        // Arrange
        var oldMqSpecialism = MandatoryQualificationSpecialism.Hearing;
        var newMqSpecialism = MandatoryQualificationSpecialism.Visual;
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithSpecialism(oldMqSpecialism)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqSpecialismState
            {
                Specialism = newMqSpecialism,
                CurrentSpecialism = oldMqSpecialism,
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
                ProvideAdditionalInformation = false,
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/specialism/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var changeSummary = doc.GetElementByTestId("change-summary");
        Assert.NotNull(changeSummary);
        Assert.Equal(oldMqSpecialism.GetTitle(), changeSummary.GetElementByTestId("current-specialism")!.TrimmedText());
        Assert.Equal(newMqSpecialism.GetTitle(), changeSummary.GetElementByTestId("new-specialism")!.TrimmedText());
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
    public async Task Post_Confirm_UpdatesMqCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var oldMqSpecialism = MandatoryQualificationSpecialism.Hearing;
        var newMqSpecialism = MandatoryQualificationSpecialism.Visual;

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithSpecialism(oldMqSpecialism)));
        var qualification = person.MandatoryQualifications.First();
        var qualificationId = qualification.QualificationId;
        var provider = MandatoryQualificationProvider.GetById(qualification.ProviderId!.Value);
        var changeReason = MqChangeSpecialismReasonOption.AnotherReason;
        var changeReasonDetail = "Some reason";

        EventObserver.Clear();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqSpecialismState
            {
                Specialism = newMqSpecialism,
                CurrentSpecialism = oldMqSpecialism,
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
                ProvideAdditionalInformation = false,
                AdditionalInformation = null,
                Evidence = new()
                {
                    UploadEvidence = false
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/specialism/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            Assert.Equal(newMqSpecialism, qualification.Specialism);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.MandatoryQualificationUpdating, p.ProcessContext.ProcessType);
            Assert.Equal(GetCurrentUserId(), p.ProcessContext.Process.UserId);
            p.AssertProcessHasEvent<MandatoryQualificationUpdatedEvent>(e =>
            {
                Assert.Equal(person.PersonId, e.PersonId);
                Assert.Equal(qualificationId, e.MandatoryQualification.QualificationId);
                Assert.Equal(MandatoryQualificationUpdatedEventChanges.Specialism, e.Changes);
                Assert.Equal(newMqSpecialism, e.MandatoryQualification.Specialism);
                Assert.Equal(oldMqSpecialism, e.OldMandatoryQualification.Specialism);
            });

            var changeReasonInfo = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(p.ProcessContext.Process.ChangeReason);
            Assert.Equal(changeReason.GetDisplayName(), changeReasonInfo.Reason);
            Assert.Equal(changeReasonDetail, changeReasonInfo.Details);
            Assert.Null(changeReasonInfo.AdditionalInformation);
            Assert.Null(changeReasonInfo.EvidenceFile);
        });
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyRedirectsAndDoesNotUpdateMq()
    {
        // Arrange
        var oldMqSpecialism = MandatoryQualificationSpecialism.Hearing;
        var newMqSpecialism = MandatoryQualificationSpecialism.Visual;
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithSpecialism(oldMqSpecialism)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqSpecialismState
            {
                Specialism = newMqSpecialism,
                ChangeReason = MqChangeSpecialismReasonOption.ChangeOfSpecialism,
                ChangeReasonDetail = null,
                ProvideAdditionalInformation = false,
                Evidence = new()
                {
                    UploadEvidence = false
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/specialism/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            Assert.Equal(oldMqSpecialism, qualification.Specialism);
        });
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var oldMqSpecialism = MandatoryQualificationSpecialism.Hearing;
        var newMqSpecialism = MandatoryQualificationSpecialism.Visual;
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithSpecialism(oldMqSpecialism)));
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqSpecialismState
            {
                Specialism = newMqSpecialism,
                ChangeReason = MqChangeSpecialismReasonOption.ChangeOfSpecialism,
                ChangeReasonDetail = "Some reason",
                Evidence = new()
                {
                    UploadEvidence = false
                }

            });

        var request = new HttpRequestMessage(httpMethod, $"/mqs/{qualificationId}/specialism/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

}
