using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Status;

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
            new EditMqStatusState()
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/status/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/status?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

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

        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus, oldEndDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqStatusState()
            {
                Initialized = true,
                CurrentStatus = oldStatus,
                Status = newStatus,
                CurrentEndDate = oldEndDate,
                EndDate = newEndDate,
                StatusChangeReason = statusChangeReason,
                EndDateChangeReason = endDateChangeReason,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = uploadEvidence,
                EvidenceFileId = uploadEvidence ? Guid.NewGuid() : null,
                EvidenceFileName = uploadEvidence ? "test.pdf" : null,
                EvidenceFileSizeDescription = uploadEvidence ? "1MB" : null
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/status/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        var changeSummary = doc.GetElementByTestId("change-summary");
        Assert.NotNull(changeSummary);

        if (isStatusChange)
        {
            Assert.Equal(oldStatus.HasValue ? oldStatus.Value.GetTitle() : "None", changeSummary.GetElementByTestId("current-status")!.TextContent);
            Assert.Equal(newStatus.GetTitle(), changeSummary.GetElementByTestId("new-status")!.TextContent);
        }
        else
        {
            Assert.Null(changeSummary.GetElementByTestId("current-status"));
            Assert.Null(changeSummary.GetElementByTestId("new-status"));
        }

        if (isEndDateChange)
        {
            Assert.Equal(oldEndDate.HasValue ? oldEndDate.Value.ToString("d MMMM yyyy") : "None", changeSummary.GetElementByTestId("current-end-date")!.TextContent);
            Assert.Equal(newEndDate.HasValue ? newEndDate.Value.ToString("d MMMM yyyy") : "None", changeSummary.GetElementByTestId("new-end-date")!.TextContent);
        }
        else
        {
            Assert.Null(changeSummary.GetElementByTestId("current-end-date"));
            Assert.Null(changeSummary.GetElementByTestId("new-end-date"));
        }

        Assert.Equal(changeReason, changeSummary.GetElementByTestId("change-reason")!.TextContent);
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
            new EditMqStatusState()
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/status?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
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

        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus, oldEndDate)));
        var qualification = person.MandatoryQualifications.First();
        var qualificationId = qualification.QualificationId;
        var provider = MandatoryQualificationProvider.GetById(qualification.ProviderId!.Value);

        EventObserver.Clear();

        Guid? evidenceFileId = uploadEvidence ? Guid.NewGuid() : null;
        string? evidenceFileName = uploadEvidence ? "test.pdf" : null;

        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqStatusState()
            {
                Initialized = true,
                CurrentStatus = oldStatus,
                Status = newStatus,
                CurrentEndDate = oldEndDate,
                EndDate = newEndDate,
                StatusChangeReason = statusChangeReason,
                EndDateChangeReason = endDateChangeReason,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = uploadEvidence,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName,
                EvidenceFileSizeDescription = uploadEvidence ? "1MB" : null
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            Assert.Equal(newStatus, qualification.Status);
            Assert.Equal(newEndDate, qualification.EndDate);
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
                        MandatoryQualificationProviderId = provider.MandatoryQualificationProviderId,
                        Name = provider.Name,
                        DqtMqEstablishmentId = null,
                        DqtMqEstablishmentName = null
                    },
                    Specialism = qualification.Specialism,
                    Status = newStatus,
                    StartDate = qualification.StartDate,
                    EndDate = newEndDate
                },
                OldMandatoryQualification = new()
                {
                    QualificationId = qualificationId,
                    Provider = new()
                    {
                        MandatoryQualificationProviderId = provider.MandatoryQualificationProviderId,
                        Name = provider.Name,
                        DqtMqEstablishmentId = null,
                        DqtMqEstablishmentName = null
                    },
                    Specialism = qualification.Specialism,
                    Status = oldStatus,
                    StartDate = qualification.StartDate,
                    EndDate = oldEndDate
                },
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
                EvidenceFile = uploadEvidence ?
                    new Core.Events.Models.File()
                    {
                        FileId = evidenceFileId!.Value,
                        Name = evidenceFileName!
                    } :
                    null,
                Changes = changes
            };

            var actualMqUpdatedEvent = Assert.IsType<MandatoryQualificationUpdatedEvent>(e);
            Assert.Equivalent(expectedMqUpdatedEvent with { EventId = actualMqUpdatedEvent.EventId }, actualMqUpdatedEvent);
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
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqStatusState()
            {
                Initialized = true,
                Status = newStatus,
                EndDate = newEndDate,
                CurrentStatus = oldStatus,
                CurrentEndDate = oldEndDate,
                UploadEvidence = false,
                StatusChangeReason = MqChangeStatusReasonOption.ChangeOfStatus
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/confirm/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            Assert.Equal(oldStatus, qualification.Status);
            Assert.Equal(oldEndDate, qualification.EndDate);
        });
    }

    private async Task<JourneyInstance<EditMqStatusState>> CreateJourneyInstance(Guid qualificationId, EditMqStatusState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqStatus,
            state ?? new EditMqStatusState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
