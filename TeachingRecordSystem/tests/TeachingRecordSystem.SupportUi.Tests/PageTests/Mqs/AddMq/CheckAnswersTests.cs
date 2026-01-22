using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.AddMq;

public class CheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(personId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/check-answers?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithPersonIdForValidPersonReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var provider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var startDate = new DateOnly(2021, 3, 1);
        var status = MandatoryQualificationStatus.Passed;
        DateOnly? endDate = new DateOnly(2021, 11, 5);
        var addReason = AddMqReasonOption.NewInformationReceived;
        var addReasonDetail = "More details about the MQ";
        var evidence = new EvidenceUploadModel
        {
            UploadEvidence = true,
            UploadedEvidenceFile = new()
            {
                FileId = Guid.NewGuid(),
                FileName = "evidence.jpeg",
                FileSizeDescription = "5MB"
            }
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddMqState
            {
                ProviderId = provider.MandatoryQualificationProviderId,
                Specialism = specialism,
                StartDate = startDate,
                Status = status,
                EndDate = endDate,
                AddReason = addReason,
                HasAdditionalReasonDetail = true,
                AddReasonDetail = addReasonDetail,
                Evidence = evidence
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(MandatoryQualificationStatus.InProgress)]
    [InlineData(MandatoryQualificationStatus.Failed)]
    [InlineData(MandatoryQualificationStatus.Passed)]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState(MandatoryQualificationStatus status)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var provider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var startDate = new DateOnly(2021, 3, 1);
        DateOnly? endDate = status == MandatoryQualificationStatus.Passed ? new DateOnly(2021, 11, 5) : null;
        var addReason = AddMqReasonOption.NewInformationReceived;
        var addReasonDetail = "More details about the MQ";
        var evidence = new EvidenceUploadModel
        {
            UploadEvidence = true,
            UploadedEvidenceFile = new()
            {
                FileId = Guid.NewGuid(),
                FileName = "evidence.jpeg",
                FileSizeDescription = "5MB"
            }
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddMqState
            {
                ProviderId = provider.MandatoryQualificationProviderId,
                Specialism = specialism,
                StartDate = startDate,
                Status = status,
                EndDate = endDate,
                AddReason = addReason,
                HasAdditionalReasonDetail = true,
                AddReasonDetail = addReasonDetail,
                Evidence = evidence
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(provider.Name, doc.GetElementByTestId("provider")!.TrimmedText());
        Assert.Equal(specialism.GetTitle(), doc.GetElementByTestId("specialism")!.TrimmedText());
        Assert.Equal(startDate.ToString(WebConstants.DateOnlyDisplayFormat), doc.GetElementByTestId("start-date")!.TrimmedText());
        Assert.Equal(status.GetTitle(), doc.GetElementByTestId("status")!.TrimmedText());
        if (status == MandatoryQualificationStatus.Passed)
        {
            Assert.Equal(endDate!.Value.ToString(WebConstants.DateOnlyDisplayFormat), doc.GetElementByTestId("end-date")!.TrimmedText());
        }
        else
        {
            Assert.Equal("None", doc.GetElementByTestId("end-date")!.TrimmedText());
        }
    }

    [Fact]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(personId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/check-answers?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(MandatoryQualificationStatus.InProgress)]
    [InlineData(MandatoryQualificationStatus.Failed)]
    [InlineData(MandatoryQualificationStatus.Passed)]
    public async Task Post_Confirm_CreatesMqCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(MandatoryQualificationStatus status)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var provider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var startDate = new DateOnly(2021, 3, 1);
        DateOnly? endDate = status == MandatoryQualificationStatus.Passed ? new DateOnly(2021, 11, 5) : null;
        var addReason = AddMqReasonOption.NewInformationReceived;
        var addReasonDetail = "More details about the MQ";
        var evidence = new EvidenceUploadModel
        {
            UploadEvidence = true,
            UploadedEvidenceFile = new()
            {
                FileId = Guid.NewGuid(),
                FileName = "evidence.jpeg",
                FileSizeDescription = "5MB"
            }
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddMqState
            {
                ProviderId = provider.MandatoryQualificationProviderId,
                Specialism = specialism,
                StartDate = startDate,
                Status = status,
                EndDate = endDate,
                AddReason = addReason,
                HasAdditionalReasonDetail = true,
                AddReasonDetail = addReasonDetail,
                Evidence = evidence
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Mandatory qualification added");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);

        Guid qualificationId = default;
        await WithDbContextAsync(async dbContext =>
        {
            var qualification = await dbContext.MandatoryQualifications.SingleOrDefaultAsync(q => q.PersonId == person.PersonId);
            Assert.NotNull(qualification);
            Assert.Equal(specialism, qualification.Specialism);
            Assert.Equal(status, qualification.Status);
            Assert.Equal(startDate, qualification.StartDate);
            Assert.Equal(endDate, qualification.EndDate);

            qualificationId = qualification.QualificationId;
        });

        EventObserver.AssertEventsSaved(e =>
        {
            var expectedMqCreatedEvent = new MandatoryQualificationCreatedEvent
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
                    Specialism = specialism,
                    Status = status,
                    StartDate = startDate,
                    EndDate = endDate
                },
                AddReason = addReason.GetDisplayName(),
                AddReasonDetail = addReasonDetail,
                EvidenceFile = evidence.UploadedEvidenceFile?.ToEventModel()
            };

            var actualMqCreatedEvent = Assert.IsType<MandatoryQualificationCreatedEvent>(e);
            Assert.Equivalent(expectedMqCreatedEvent with { EventId = actualMqCreatedEvent.EventId }, actualMqCreatedEvent);
        });
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyRedirectsAndDoesNotCreateMq()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var provider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var startDate = new DateOnly(2021, 3, 1);
        var status = MandatoryQualificationStatus.Passed;
        DateOnly? endDate = new DateOnly(2021, 11, 5);
        var addReason = AddMqReasonOption.NewInformationReceived;
        var addReasonDetail = "More details about the MQ";
        var evidence = new EvidenceUploadModel
        {
            UploadEvidence = true,
            UploadedEvidenceFile = new()
            {
                FileId = Guid.NewGuid(),
                FileName = "evidence.jpeg",
                FileSizeDescription = "5MB"
            }
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddMqState
            {
                ProviderId = provider.MandatoryQualificationProviderId,
                Specialism = specialism,
                StartDate = startDate,
                Status = status,
                EndDate = endDate,
                AddReason = addReason,
                HasAdditionalReasonDetail = true,
                AddReasonDetail = addReasonDetail,
                Evidence = evidence
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/check-answers/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
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
            var qualifications = await dbContext.MandatoryQualifications.Where(q => q.PersonId == person.PersonId).ToArrayAsync();
            Assert.Empty(qualifications);
        });
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var provider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Leeds");
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var startDate = new DateOnly(2021, 3, 1);
        var status = MandatoryQualificationStatus.Passed;
        DateOnly? endDate = new DateOnly(2021, 11, 5);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddMqState
            {
                ProviderId = provider.MandatoryQualificationProviderId,
                Specialism = specialism,
                StartDate = startDate,
                Status = status,
                EndDate = endDate
            });

        var request = new HttpRequestMessage(httpMethod, $"/mqs/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private async Task<JourneyInstance<AddMqState>> CreateJourneyInstanceAsync(Guid personId, AddMqState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.AddMq,
            state ?? new AddMqState(),
            new KeyValuePair<string, object>("personId", personId));
}
