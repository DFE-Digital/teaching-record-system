using FormFlow;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.AddMq;

public class CheckAnswersTests : TestBase
{
    public CheckAnswersTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(personId);

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
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5), "212", new DateTime(2021, 10, 5)));

        var mqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue("959"); // University of Leeds
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var startDate = new DateOnly(2021, 3, 1);
        var status = MandatoryQualificationStatus.Passed;
        DateOnly? endDate = new DateOnly(2021, 11, 5);

        var journeyInstance = await CreateJourneyInstance(
            person.ContactId,
            new AddMqState()
            {
                MqEstablishmentValue = mqEstablishment.dfeta_Value,
                Specialism = specialism,
                StartDate = startDate,
                Status = status,
                EndDate = endDate
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
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5), "212", new DateTime(2021, 10, 5)));

        var mqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue("959"); // University of Leeds
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var startDate = new DateOnly(2021, 3, 1);
        DateOnly? endDate = status == MandatoryQualificationStatus.Passed ? new DateOnly(2021, 11, 5) : null;

        var journeyInstance = await CreateJourneyInstance(
            person.ContactId,
            new AddMqState()
            {
                MqEstablishmentValue = mqEstablishment.dfeta_Value,
                Specialism = specialism,
                StartDate = startDate,
                Status = status,
                EndDate = endDate
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocument();
        Assert.Equal(mqEstablishment.dfeta_name, doc.GetElementByTestId("provider")!.TextContent);
        Assert.Equal(specialism.GetTitle(), doc.GetElementByTestId("specialism")!.TextContent);
        Assert.Equal(startDate.ToString("d MMMM yyyy"), doc.GetElementByTestId("start-date")!.TextContent);
        Assert.Equal(status.ToString(), doc.GetElementByTestId("status")!.TextContent);
        if (status == MandatoryQualificationStatus.Passed)
        {
            Assert.Equal(endDate!.Value.ToString("d MMMM yyyy"), doc.GetElementByTestId("end-date")!.TextContent);
        }
        else
        {
            Assert.Equal("None", doc.GetElementByTestId("end-date")!.TextContent);
        }
    }

    [Fact]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(personId);

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
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5), "212", new DateTime(2021, 10, 5)));

        var mqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue("959"); // University of Leeds
        MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(mqEstablishment, out var provider);
        Assert.NotNull(provider);

        var specialism = MandatoryQualificationSpecialism.Hearing;
        var startDate = new DateOnly(2021, 3, 1);
        DateOnly? endDate = status == MandatoryQualificationStatus.Passed ? new DateOnly(2021, 11, 5) : null;

        var journeyInstance = await CreateJourneyInstance(
            person.ContactId,
            new AddMqState()
            {
                MqEstablishmentValue = mqEstablishment.dfeta_Value,
                Specialism = specialism,
                StartDate = startDate,
                Status = status,
                EndDate = endDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Mandatory qualification added");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);

        Guid qualificationId = default;
        await WithDbContext(async dbContext =>
        {
            var qualification = await dbContext.MandatoryQualifications.SingleOrDefaultAsync(q => q.PersonId == person.PersonId);
            Assert.NotNull(qualification);
            Assert.Equal(provider!.MandatoryQualificationProviderId, qualification.ProviderId);
            Assert.Equal(specialism, qualification.Specialism);
            Assert.Equal(status, qualification.Status);
            Assert.Equal(startDate, qualification.StartDate);
            Assert.Equal(endDate, qualification.EndDate);

            qualificationId = qualification.QualificationId;
        });

        EventObserver.AssertEventsSaved(e =>
        {
            var expectedMqCreatedEvent = new MandatoryQualificationCreatedEvent()
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
                        MandatoryQualificationProviderId = null,
                        Name = null,
                        DqtMqEstablishmentId = mqEstablishment.Id,
                        DqtMqEstablishmentName = mqEstablishment.dfeta_name
                    },
                    Specialism = specialism,
                    Status = status,
                    StartDate = startDate,
                    EndDate = endDate
                }
            };

            var actualMqCreatedEvent = Assert.IsType<MandatoryQualificationCreatedEvent>(e);
            Assert.Equivalent(expectedMqCreatedEvent with { EventId = actualMqCreatedEvent.EventId }, actualMqCreatedEvent);
        });
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyRedirectsAndDoesNotCreateMq()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5), "212", new DateTime(2021, 10, 5)));
        var mqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue("959"); // University of Leeds
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var startDate = new DateOnly(2021, 3, 1);
        var status = MandatoryQualificationStatus.Passed;
        DateOnly? endDate = new DateOnly(2021, 11, 5);
        var journeyInstance = await CreateJourneyInstance(
            person.ContactId,
            new AddMqState()
            {
                MqEstablishmentValue = mqEstablishment.dfeta_Value,
                Specialism = specialism,
                StartDate = startDate,
                Status = status,
                EndDate = endDate
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

        await WithDbContext(async dbContext =>
        {
            var qualifications = await dbContext.MandatoryQualifications.Where(q => q.PersonId == person.PersonId).ToArrayAsync();
            Assert.Empty(qualifications);
        });
    }

    private async Task<JourneyInstance<AddMqState>> CreateJourneyInstance(Guid personId, AddMqState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.AddMq,
            state ?? new AddMqState(),
            new KeyValuePair<string, object>("personId", personId));
}
