using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Specialism;

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
            new EditMqSpecialismState
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/specialism/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("Some reason", true)]
    public async Task Get_ValidRequest_DisplaysContentAsExpected(
        string? changeReasonDetail,
        bool uploadEvidence)
    {
        // Arrange
        var oldMqSpecialism = MandatoryQualificationSpecialism.Hearing;
        var newMqSpecialism = MandatoryQualificationSpecialism.Visual;
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithSpecialism(oldMqSpecialism)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var changeReason = MqChangeSpecialismReasonOption.ChangeOfSpecialism;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqSpecialismState
            {
                Initialized = true,
                Specialism = newMqSpecialism,
                CurrentSpecialism = oldMqSpecialism,
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
    public async Task Post_MissingDataInJourneyState_Redirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqSpecialismState
            {
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/specialism/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
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
        var changeReason = MqChangeSpecialismReasonOption.ChangeOfSpecialism;
        var changeReasonDetail = "Some reason";

        EventObserver.Clear();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqSpecialismState
            {
                Initialized = true,
                Specialism = newMqSpecialism,
                CurrentSpecialism = oldMqSpecialism,
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
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
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Mandatory qualification changed");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);

        await WithDbContextAsync(async dbContext =>
        {
            var qualification = await dbContext.MandatoryQualifications.SingleAsync(q => q.PersonId == person.PersonId);
            Assert.Equal(newMqSpecialism, qualification.Specialism);
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
                    Specialism = newMqSpecialism,
                    Status = qualification.Status,
                    StartDate = qualification.StartDate,
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
                    Specialism = oldMqSpecialism,
                    Status = qualification.Status,
                    StartDate = qualification.StartDate,
                    EndDate = qualification.EndDate
                },
                ChangeReason = changeReason.GetDisplayName(),
                ChangeReasonDetail = changeReasonDetail,
                EvidenceFile = null,
                Changes = MandatoryQualificationUpdatedEventChanges.Specialism
            };

            var actualMqUpdatedEvent = Assert.IsType<MandatoryQualificationUpdatedEvent>(e);
            Assert.Equivalent(expectedMqUpdatedEvent with { EventId = actualMqUpdatedEvent.EventId }, actualMqUpdatedEvent);
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
                Initialized = true,
                Specialism = newMqSpecialism,
                ChangeReason = MqChangeSpecialismReasonOption.ChangeOfSpecialism,
                ChangeReasonDetail = "Some reason",
                Evidence = new()
                {
                    UploadEvidence = false
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/specialism/check-answers/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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
                Initialized = true,
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

    private async Task<JourneyInstance<EditMqSpecialismState>> CreateJourneyInstanceAsync(Guid qualificationId, EditMqSpecialismState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqSpecialism,
            state ?? new EditMqSpecialismState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
