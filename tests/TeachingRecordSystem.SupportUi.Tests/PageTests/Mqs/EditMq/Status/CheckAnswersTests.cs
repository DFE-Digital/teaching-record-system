using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Status;

public class CheckAnswersTests(HostFixture hostFixture) : EditMqStatusTestBase(hostFixture)
{
    [Theory]
    [InlineData(true, false, "some reason", true)]
    [InlineData(false, true, null, true)]
    [InlineData(true, true, null, false)]
    public async Task Get_ValidRequest_DisplaysContentAsExpected(
        bool isStatusChange,
        bool isEndDateChange,
        string? changeReasonDetail,
        bool uploadEvidence)
    {
        // Arrange
        MandatoryQualificationStatus? oldStatus;
        MandatoryQualificationStatus newStatus;
        DateOnly? oldEndDate;
        DateOnly? newEndDate;
        MqChangeStatusReasonOption? statusChangeReason = null;
        MqChangeEndDateReasonOption? endDateChangeReason = null;
        string changeReason = "";

        if (isStatusChange)
        {
            if (isEndDateChange)
            {
                oldStatus = MandatoryQualificationStatus.Failed;
                newStatus = MandatoryQualificationStatus.Passed;
                oldEndDate = null;
                newEndDate = new DateOnly(2021, 12, 5);
                statusChangeReason = MqChangeStatusReasonOption.ChangeOfStatus;
                changeReason = statusChangeReason.GetDisplayName()!;
            }
            else
            {
                oldStatus = null;
                newStatus = MandatoryQualificationStatus.Failed;
                oldEndDate = null;
                newEndDate = null;
                statusChangeReason = MqChangeStatusReasonOption.ChangeOfStatus;
                changeReason = statusChangeReason.GetDisplayName()!;
            }
        }
        else
        {
            oldStatus = MandatoryQualificationStatus.Passed;
            newStatus = MandatoryQualificationStatus.Passed;
            oldEndDate = new DateOnly(2021, 12, 5);
            newEndDate = new DateOnly(2021, 12, 6);
            endDateChangeReason = MqChangeEndDateReasonOption.ChangeOfEndDate;
            changeReason = endDateChangeReason.GetDisplayName()!;
        }

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus, oldEndDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStatusState
            {
                CurrentStatus = oldStatus,
                Status = newStatus,
                CurrentEndDate = oldEndDate,
                EndDate = newEndDate,
                StatusChangeReason = statusChangeReason,
                EndDateChangeReason = endDateChangeReason,
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
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/status/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var changeSummary = doc.GetElementByTestId("change-summary");
        Assert.NotNull(changeSummary);

        if (isStatusChange)
        {
            Assert.Equal(oldStatus.HasValue ? oldStatus.Value.GetTitle() : "None", changeSummary.GetElementByTestId("current-status")!.TrimmedText());
            Assert.Equal(newStatus.GetTitle(), changeSummary.GetElementByTestId("new-status")!.TrimmedText());
        }
        else
        {
            Assert.Null(changeSummary.GetElementByTestId("current-status"));
            Assert.Null(changeSummary.GetElementByTestId("new-status"));
        }

        if (isEndDateChange)
        {
            Assert.Equal(oldEndDate.HasValue ? oldEndDate.Value.ToString(WebConstants.DateDisplayFormat) : "None", changeSummary.GetElementByTestId("current-end-date")!.TrimmedText());
            Assert.Equal(newEndDate.HasValue ? newEndDate.Value.ToString(WebConstants.DateDisplayFormat) : "None", changeSummary.GetElementByTestId("new-end-date")!.TrimmedText());
        }
        else
        {
            Assert.Null(changeSummary.GetElementByTestId("current-end-date"));
            Assert.Null(changeSummary.GetElementByTestId("new-end-date"));
        }

        var changeReasonSummary = doc.GetElementByTestId("change-reason-summary");
        Assert.NotNull(changeReasonSummary);
        Assert.Equal(changeReason, changeReasonSummary.GetElementByTestId("change-reason")!.TrimmedText());
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

    [Theory]
    [InlineData(true, false, "some reason", true)]
    [InlineData(false, true, null, true)]
    [InlineData(true, true, null, false)]
    public async Task Post_Confirm_UpdatesMqCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(
        bool isStatusChange,
        bool isEndDateChange,
        string? changeReasonDetail,
        bool uploadEvidence)
    {
        // Arrange
        MandatoryQualificationStatus? oldStatus;
        MandatoryQualificationStatus newStatus;
        DateOnly? oldEndDate;
        DateOnly? newEndDate;
        MqChangeStatusReasonOption? statusChangeReason = null;
        MqChangeEndDateReasonOption? endDateChangeReason = null;
        string changeReason = "";
        var changes = MandatoryQualificationUpdatedEventChanges.None;

        if (isStatusChange)
        {
            if (isEndDateChange)
            {
                oldStatus = MandatoryQualificationStatus.Failed;
                newStatus = MandatoryQualificationStatus.Passed;
                oldEndDate = null;
                newEndDate = new DateOnly(2021, 12, 5);
                statusChangeReason = MqChangeStatusReasonOption.ChangeOfStatus;
                changeReason = statusChangeReason.GetDisplayName()!;
                changes = MandatoryQualificationUpdatedEventChanges.Status | MandatoryQualificationUpdatedEventChanges.EndDate;
            }
            else
            {
                oldStatus = null;
                newStatus = MandatoryQualificationStatus.Failed;
                oldEndDate = null;
                newEndDate = null;
                statusChangeReason = MqChangeStatusReasonOption.ChangeOfStatus;
                changeReason = statusChangeReason.GetDisplayName()!;
                changes = MandatoryQualificationUpdatedEventChanges.Status;
            }
        }
        else
        {
            oldStatus = MandatoryQualificationStatus.Passed;
            newStatus = MandatoryQualificationStatus.Passed;
            oldEndDate = new DateOnly(2021, 12, 5);
            newEndDate = new DateOnly(2021, 12, 6);
            endDateChangeReason = MqChangeEndDateReasonOption.ChangeOfEndDate;
            changeReason = endDateChangeReason.GetDisplayName()!;
            changes = MandatoryQualificationUpdatedEventChanges.EndDate;
        }

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus, oldEndDate)));
        var qualification = person.MandatoryQualifications.First();
        var qualificationId = qualification.QualificationId;
        var provider = MandatoryQualificationProvider.GetById(qualification.ProviderId!.Value);

        EventObserver.Clear();

        Guid evidenceFileId = Guid.NewGuid();
        string evidenceFileName = "test.pdf";
        var additionalInformation = "Some more details";

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStatusState
            {
                CurrentStatus = oldStatus,
                Status = newStatus,
                CurrentEndDate = oldEndDate,
                EndDate = newEndDate,
                StatusChangeReason = statusChangeReason,
                EndDateChangeReason = endDateChangeReason,
                ChangeReasonDetail = changeReasonDetail,
                Evidence = new()
                {
                    UploadEvidence = uploadEvidence,
                    UploadedEvidenceFile = uploadEvidence ? new()
                    {
                        FileId = evidenceFileId,
                        FileName = evidenceFileName,
                        FileSizeDescription = "1MB"
                    } : null
                },
                ProvideAdditionalInformation = true,
                AdditionalInformation = additionalInformation
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            Assert.Equal(newStatus, qualification.Status);
            Assert.Equal(newEndDate, qualification.EndDate);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.MandatoryQualificationUpdating, p.ProcessContext.ProcessType);
            Assert.Equal(GetCurrentUserId(), p.ProcessContext.Process.UserId);
            p.AssertProcessHasEvent<MandatoryQualificationUpdatedEvent>(e =>
            {
                Assert.Equal(person.PersonId, e.PersonId);
                Assert.Equal(qualificationId, e.MandatoryQualification.QualificationId);
                Assert.Equal(changes, e.Changes);
                Assert.Equal(newStatus, e.MandatoryQualification.Status);
                Assert.Equal(newEndDate, e.MandatoryQualification.EndDate);
                Assert.Equal(oldStatus, e.OldMandatoryQualification.Status);
                Assert.Equal(oldEndDate, e.OldMandatoryQualification.EndDate);
            });

            var changeReasonInfo = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(p.ProcessContext.Process.ChangeReason);
            Assert.Equal(changeReason, changeReasonInfo.Reason);
            Assert.Equal(changeReasonDetail, changeReasonInfo.Details);
            Assert.Equal(additionalInformation, changeReasonInfo.AdditionalInformation);
            Assert.Equal(
                uploadEvidence ? new EventModels.File { FileId = evidenceFileId, Name = evidenceFileName } : null,
                changeReasonInfo.EvidenceFile);
        });
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyRedirectsAndDoesNotUpdateMq()
    {
        // Arrange
        var oldStatus = MandatoryQualificationStatus.Failed;
        var oldEndDate = (DateOnly?)null;
        var newStatus = MandatoryQualificationStatus.Passed;
        var newEndDate = new DateOnly(2021, 12, 5);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStatusState
            {
                Status = newStatus,
                EndDate = newEndDate,
                CurrentStatus = oldStatus,
                CurrentEndDate = oldEndDate,
                Evidence = new()
                {
                    UploadEvidence = false,
                },
                StatusChangeReason = MqChangeStatusReasonOption.ChangeOfStatus
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            Assert.Equal(oldStatus, qualification.Status);
            Assert.Equal(oldEndDate, qualification.EndDate);
        });
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var oldStatus = MandatoryQualificationStatus.Failed;
        var oldEndDate = (DateOnly?)null;
        var newStatus = MandatoryQualificationStatus.Passed;
        var newEndDate = new DateOnly(2021, 12, 5);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus)));
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStatusState
            {
                Status = newStatus,
                EndDate = newEndDate,
                CurrentStatus = oldStatus,
                CurrentEndDate = oldEndDate,
                Evidence = new()
                {
                    UploadEvidence = false
                },
                StatusChangeReason = MqChangeStatusReasonOption.ChangeOfStatus
            });

        var request = new HttpRequestMessage(httpMethod, $"/mqs/{qualificationId}/status/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

}
