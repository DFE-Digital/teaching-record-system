using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.StartDate;

public class CheckAnswersTests(HostFixture hostFixture) : EditMqStartDateTestBase(hostFixture)
{
    [Theory]
    [InlineData(MqChangeStartDateReasonOption.IncorrectStartDate, null, false, true, "additional information")]
    [InlineData(MqChangeStartDateReasonOption.AnotherReason, "Some reason", true, false, null)]
    public async Task Get_ValidRequest_DisplaysContentAsExpected(
        MqChangeStartDateReasonOption changeReason,
        string? changeReasonDetail,
        bool uploadEvidence,
        bool provideAdditionalInformation,
        string? additionalInformation)
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate,
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var changeSummary = doc.GetElementByTestId("change-summary");
        Assert.NotNull(changeSummary);
        Assert.Equal(oldStartDate.ToString(WebConstants.DateDisplayFormat), changeSummary.GetElementByTestId("current-start-date")!.TrimmedText());
        Assert.Equal(newStartDate.ToString(WebConstants.DateDisplayFormat), changeSummary.GetElementByTestId("new-start-date")!.TrimmedText());
        var changeReasonSummary = doc.GetElementByTestId("change-reason-summary");
        Assert.NotNull(changeReasonSummary);
        Assert.Equal(changeReason.GetDisplayName(), changeReasonSummary.GetElementByTestId("change-reason")!.TrimmedText());
        Assert.Equal(!string.IsNullOrEmpty(changeReasonDetail) ? changeReasonDetail : "None", changeReasonSummary.GetElementByTestId("change-reason-detail")!.TrimmedText());
        Assert.Equal(provideAdditionalInformation == true ? additionalInformation : "None", changeReasonSummary.GetElementByTestId("additional-information")!.TrimmedText());
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
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualification = person.MandatoryQualifications.First();
        var qualificationId = qualification.QualificationId;
        var provider = MandatoryQualificationProvider.GetById(qualification.ProviderId!.Value);
        var changeReason = MqChangeStartDateReasonOption.AnotherReason;
        var changeReasonDetail = "Some reason";

        EventObserver.Clear();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate,
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
                Evidence = new()
                {
                    UploadEvidence = false
                },
                ProvideAdditionalInformation = false,
                AdditionalInformation = null
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            Assert.Equal(newStartDate, qualification.StartDate);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.MandatoryQualificationUpdating, p.ProcessContext.ProcessType);
            Assert.Equal(GetCurrentUserId(), p.ProcessContext.Process.UserId);
            p.AssertProcessHasEvent<MandatoryQualificationUpdatedEvent>(e =>
            {
                Assert.Equal(person.PersonId, e.PersonId);
                Assert.Equal(qualificationId, e.MandatoryQualification.QualificationId);
                Assert.Equal(MandatoryQualificationUpdatedEventChanges.StartDate, e.Changes);
                Assert.Equal(newStartDate, e.MandatoryQualification.StartDate);
                Assert.Equal(oldStartDate, e.OldMandatoryQualification.StartDate);
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
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate,
                ChangeReason = MqChangeStartDateReasonOption.IncorrectStartDate,
                ChangeReasonDetail = null,
                Evidence = new()
                {
                    UploadEvidence = false
                },
                ProvideAdditionalInformation = false,
                AdditionalInformation = null
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            Assert.Equal(oldStartDate, qualification.StartDate);
        });
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate,
                ChangeReason = MqChangeStartDateReasonOption.IncorrectStartDate,
                ChangeReasonDetail = "Some reason",
                Evidence = new()
                {
                    UploadEvidence = false
                }
            });

        var request = new HttpRequestMessage(httpMethod, $"/mqs/{qualificationId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

}
