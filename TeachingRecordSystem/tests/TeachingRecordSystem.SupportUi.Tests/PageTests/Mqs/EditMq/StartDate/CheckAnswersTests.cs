using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.StartDate;

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
            new EditMqStartDateState
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var changeReason = MqChangeStartDateReasonOption.IncorrectStartDate;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                Initialized = true,
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
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var changeSummary = doc.GetElementByTestId("change-summary");
        Assert.NotNull(changeSummary);
        Assert.Equal(oldStartDate.ToString(UiDefaults.DateOnlyDisplayFormat), changeSummary.GetElementByTestId("current-start-date")!.TrimmedText());
        Assert.Equal(newStartDate.ToString(UiDefaults.DateOnlyDisplayFormat), changeSummary.GetElementByTestId("new-start-date")!.TrimmedText());
        Assert.Equal(changeReason.GetDisplayName(), changeSummary.GetElementByTestId("change-reason")!.TrimmedText());
        Assert.Equal(!string.IsNullOrEmpty(changeReasonDetail) ? changeReasonDetail : "None", changeSummary.GetElementByTestId("change-reason-detail")!.TrimmedText());
        var uploadedEvidenceLink = changeSummary.GetElementByTestId("uploaded-evidence-file-link");
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
            new EditMqStartDateState
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
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

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualification = person.MandatoryQualifications.First();
        var qualificationId = qualification.QualificationId;
        var provider = MandatoryQualificationProvider.GetById(qualification.ProviderId!.Value);
        var changeReason = MqChangeStartDateReasonOption.IncorrectStartDate;
        var changeReasonDetail = "Some reason";

        EventObserver.Clear();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate,
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
                Evidence = new()
                {
                    UploadEvidence = false
                }
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
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Mandatory qualification changed");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);

        await WithDbContextAsync(async dbContext =>
        {
            var qualification = await dbContext.MandatoryQualifications.SingleAsync(q => q.PersonId == person.PersonId);
            Assert.Equal(newStartDate, qualification.StartDate);
        });

        EventObserver.AssertEventsSaved(e =>
        {
            var expectedMqUpdatedEvent = new MandatoryQualificationUpdatedEvent
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
                        DqtMqEstablishmentName = null,
                        DqtMqEstablishmentValue = null
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
                        DqtMqEstablishmentName = null,
                        DqtMqEstablishmentValue = null
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
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                Initialized = true,
                StartDate = newStartDate,
                CurrentStartDate = oldStartDate,
                ChangeReason = MqChangeStartDateReasonOption.IncorrectStartDate,
                ChangeReasonDetail = "Some reason",
                Evidence = new()
                {
                    UploadEvidence = false
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/check-answers/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);

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
                Initialized = true,
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

    private async Task<JourneyInstance<EditMqStartDateState>> CreateJourneyInstanceAsync(Guid qualificationId, EditMqStartDateState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqStartDate,
            state ?? new EditMqStartDateState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
