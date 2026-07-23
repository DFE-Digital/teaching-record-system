using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.AddMq;

public class CheckAnswersTests(HostFixture hostFixture) : AddMqTestBase(hostFixture)
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
        var additionalInformation = "More details about the MQ";
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
                ProvideAdditionalInformation = true,
                AddReasonDetail = null,
                Evidence = evidence,
                AdditionalInformation = additionalInformation
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
        var addReason = AddMqReasonOption.AnotherReason;
        var addReasonDetail = "More details about the MQ";
        var additionalInformation = "some additional info";
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
                ProvideAdditionalInformation = true,
                AdditionalInformation = additionalInformation,
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
        Assert.Equal(startDate.ToString(WebConstants.DateDisplayFormat), doc.GetElementByTestId("start-date")!.TrimmedText());
        Assert.Equal(status.GetTitle(), doc.GetElementByTestId("status")!.TrimmedText());
        Assert.Equal(addReasonDetail, doc.GetElementByTestId("reason-details")!.TrimmedText());
        Assert.Equal(additionalInformation, doc.GetElementByTestId("additional-information")!.TrimmedText());
        if (status == MandatoryQualificationStatus.Passed)
        {
            Assert.Equal(endDate!.Value.ToString(WebConstants.DateDisplayFormat), doc.GetElementByTestId("end-date")!.TrimmedText());
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
        var addReason = AddMqReasonOption.AnotherReason;
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
                ProvideAdditionalInformation = false,
                AddReasonDetail = addReasonDetail,
                Evidence = evidence,
                AdditionalInformation = null
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
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, "Mandatory qualification added");

        Assert.Null(GetJourneyInstanceState(journeyInstance));

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

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.MandatoryQualificationCreating, p.ProcessContext.ProcessType);
            Assert.Equal(GetCurrentUserId(), p.ProcessContext.Process.UserId);
            p.AssertProcessHasEvent<MandatoryQualificationCreatedEvent>(e =>
            {
                Assert.Equal(person.PersonId, e.PersonId);
                Assert.Equal(qualificationId, e.MandatoryQualification.QualificationId);
                Assert.Equal(provider.MandatoryQualificationProviderId, e.MandatoryQualification.Provider?.MandatoryQualificationProviderId);
                Assert.Equal(provider.Name, e.MandatoryQualification.Provider?.Name);
                Assert.Equal(specialism, e.MandatoryQualification.Specialism);
                Assert.Equal(status, e.MandatoryQualification.Status);
                Assert.Equal(startDate, e.MandatoryQualification.StartDate);
                Assert.Equal(endDate, e.MandatoryQualification.EndDate);
            });

            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(p.ProcessContext.Process.ChangeReason);
            Assert.Equal(addReason.GetDisplayName(), changeReason.Reason);
            Assert.Equal(addReasonDetail, changeReason.Details);
            Assert.Null(changeReason.AdditionalInformation);
            Assert.Equal(evidence.UploadedEvidenceFile?.ToEventModel(), changeReason.EvidenceFile);
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
        var additionalInformation = "More details about the MQ";
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
                ProvideAdditionalInformation = true,
                AddReasonDetail = null,
                AdditionalInformation = additionalInformation,
                Evidence = evidence
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
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

    private Task<AddMqJourneyCoordinator> CreateJourneyInstanceAsync(Guid personId, AddMqState? state = null) =>
        CreateJourneyInstanceForStateAsync(personId, state ?? new AddMqState());
}
