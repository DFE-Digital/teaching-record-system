using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.StartDate;

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
            new EditMqStartDateState()
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/start-date/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("Some reason", true)]
    public async Task Get_ValidRequest_DisplaysContentAsExpected(
        string? changeReasonDetail,
        bool uploadEvidence)
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var changeReason = MqChangeStartDateReasonOption.IncorrectStartDate;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqStartDateState()
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate,
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = uploadEvidence,
                EvidenceFileId = uploadEvidence ? Guid.NewGuid() : null,
                EvidenceFileName = uploadEvidence ? "test.pdf" : null,
                EvidenceFileSizeDescription = uploadEvidence ? "1MB" : null
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/start-date/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var changeSummary = doc.GetElementByTestId("change-summary");
        Assert.NotNull(changeSummary);
        Assert.Equal(oldStartDate.ToString(UiDefaults.DateOnlyDisplayFormat), changeSummary.GetElementByTestId("current-start-date")!.TextContent);
        Assert.Equal(newStartDate.ToString(UiDefaults.DateOnlyDisplayFormat), changeSummary.GetElementByTestId("new-start-date")!.TextContent);
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
            new EditMqStartDateState()
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Confirm_UpdatesMqCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);

        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualification = person.MandatoryQualifications.First();
        var qualificationId = qualification.QualificationId;
        var provider = MandatoryQualificationProvider.GetById(qualification.ProviderId!.Value);
        var changeReason = MqChangeStartDateReasonOption.IncorrectStartDate;
        var changeReasonDetail = "Some reason";

        EventPublisher.Clear();

        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqStartDateState()
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate,
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
                UploadEvidence = false,
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            Assert.Equal(newStartDate, qualification.StartDate);
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
                        MandatoryQualificationProviderId = provider.MandatoryQualificationProviderId,
                        Name = provider.Name,
                        DqtMqEstablishmentId = null,
                        DqtMqEstablishmentName = null
                    },
                    Specialism = qualification.Specialism,
                    Status = qualification.Status,
                    StartDate = newStartDate,
                    EndDate = qualification.EndDate
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
                    Status = qualification.Status,
                    StartDate = oldStartDate,
                    EndDate = qualification.EndDate
                },
                ChangeReason = changeReason.GetDisplayName(),
                ChangeReasonDetail = changeReasonDetail,
                EvidenceFile = null,
                Changes = MandatoryQualificationUpdatedEventChanges.StartDate
            };

            var actualMqUpdatedEvent = Assert.IsType<MandatoryQualificationUpdatedEvent>(e);
            Assert.Equivalent(expectedMqUpdatedEvent with { EventId = actualMqUpdatedEvent.EventId }, actualMqUpdatedEvent);
        });
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyRedirectsAndDoesNotUpdateMq()
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqStartDateState()
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate,
                ChangeReason = MqChangeStartDateReasonOption.IncorrectStartDate,
                ChangeReasonDetail = "Some reason",
                UploadEvidence = false
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/confirm/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            Assert.Equal(oldStartDate, qualification.StartDate);
        });
    }

    private async Task<JourneyInstance<EditMqStartDateState>> CreateJourneyInstance(Guid qualificationId, EditMqStartDateState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqStartDate,
            state ?? new EditMqStartDateState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
