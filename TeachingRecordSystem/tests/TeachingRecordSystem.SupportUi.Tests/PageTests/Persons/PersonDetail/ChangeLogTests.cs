using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogTests : TestBase
{
    public ChangeLogTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange        
        var nonExistentPersonId = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{nonExistentPersonId}/changelog");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithNoChanges_DisplaysNoChanges()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/changelog");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var noChanges = doc.GetElementByTestId("no-changes");
        Assert.NotNull(noChanges);
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithNotesChanges_DisplaysChangesAsExpected()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        await TestData.CreateNote(b => b.WithPersonId(person.ContactId).WithSubject("Note 1 Subject").WithDescription("Note 1 Description"));
        await TestData.CreateNote(b => b.WithPersonId(person.ContactId).WithSubject("Note 2 Subject").WithDescription("Note 2 Description"));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/changelog");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var changes = doc.GetAllElementsByTestId("timeline-item");
        Assert.NotEmpty(changes);
        Assert.Equal(2, changes.Count);
        Assert.Equal("Note modified", changes[0].GetElementByTestId("timeline-item-title")!.TextContent.Trim());
        Assert.Equal("by Test User", changes[0].GetElementByTestId("timeline-item-user")!.TextContent.Trim());
        Assert.Null(changes[0].GetElementByTestId("timeline-item-status"));
        Assert.NotNull(changes[0].GetElementByTestId("timeline-item-time"));
        Assert.Equal("Note 2 Subject", changes[0].GetElementByTestId("timeline-item-summary")!.TextContent.Trim());
        Assert.Equal("Note 2 Description", changes[0].GetElementByTestId("timeline-item-description")!.TextContent.Trim());
        Assert.Equal("Note modified", changes[1].GetElementByTestId("timeline-item-title")!.TextContent.Trim());
        Assert.Equal("by Test User", changes[1].GetElementByTestId("timeline-item-user")!.TextContent.Trim());
        Assert.Null(changes[1].GetElementByTestId("timeline-item-status"));
        Assert.NotNull(changes[1].GetElementByTestId("timeline-item-time"));
        Assert.Equal("Note 1 Subject", changes[1].GetElementByTestId("timeline-item-summary")!.TextContent.Trim());
        Assert.Equal("Note 1 Description", changes[1].GetElementByTestId("timeline-item-description")!.TextContent.Trim());
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithTaskChanges_DisplaysChangesAsExpected()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        await TestData.CreateCrmTask(b => b.WithPersonId(person.ContactId).WithSubject("Task 1 Subject").WithDescription("Task 1 Description"));
        await TestData.CreateCrmTask(b => b.WithPersonId(person.ContactId).WithSubject("Task 2 Subject").WithDescription("Task 2 Description").WithDueDate(Clock.UtcNow.AddDays(-2)));
        await TestData.CreateCrmTask(b => b.WithPersonId(person.ContactId).WithSubject("Task 3 Subject").WithDescription("Task 3 Description").WithCompletedStatus());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/changelog");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var changes = doc.GetAllElementsByTestId("timeline-item");
        Assert.NotEmpty(changes);
        Assert.Equal(3, changes.Count);
        Assert.Equal("Task completed", changes[0].GetElementByTestId("timeline-item-title")!.TextContent.Trim());
        Assert.Equal("by Test User", changes[0].GetElementByTestId("timeline-item-user")!.TextContent.Trim());
        Assert.Equal("Closed", changes[0].GetElementByTestId("timeline-item-status")!.TextContent.Trim());
        Assert.NotNull(changes[0].GetElementByTestId("timeline-item-time"));
        Assert.Equal("Task 3 Subject", changes[0].GetElementByTestId("timeline-item-summary")!.TextContent.Trim());
        Assert.Equal("Task 3 Description", changes[0].GetElementByTestId("timeline-item-description")!.TextContent.Trim());
        Assert.Equal("Task modified", changes[1].GetElementByTestId("timeline-item-title")!.TextContent.Trim());
        Assert.Equal("by Test User", changes[1].GetElementByTestId("timeline-item-user")!.TextContent.Trim());
        Assert.Equal("Overdue", changes[1].GetElementByTestId("timeline-item-status")!.TextContent.Trim());
        Assert.NotNull(changes[1].GetElementByTestId("timeline-item-time"));
        Assert.Equal("Task 2 Subject", changes[1].GetElementByTestId("timeline-item-summary")!.TextContent.Trim());
        Assert.Equal("Task 2 Description", changes[1].GetElementByTestId("timeline-item-description")!.TextContent.Trim());
        Assert.Equal("Task modified", changes[2].GetElementByTestId("timeline-item-title")!.TextContent.Trim());
        Assert.Equal("by Test User", changes[2].GetElementByTestId("timeline-item-user")!.TextContent.Trim());
        Assert.Equal("Active", changes[2].GetElementByTestId("timeline-item-status")!.TextContent.Trim());
        Assert.NotNull(changes[2].GetElementByTestId("timeline-item-time"));
        Assert.Equal("Task 1 Subject", changes[2].GetElementByTestId("timeline-item-summary")!.TextContent.Trim());
        Assert.Equal("Task 1 Description", changes[2].GetElementByTestId("timeline-item-description")!.TextContent.Trim());
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithNameOrDateOfBirthChanges_DisplaysChangesAsExpected()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        await TestData.CreateNameChangeIncident(b => b.WithCustomerId(person.ContactId).WithRejectedStatus());
        await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(person.ContactId).WithApprovedStatus());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/changelog");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var changes = doc.GetAllElementsByTestId("timeline-item");
        Assert.NotEmpty(changes);
        Assert.Equal(2, changes.Count);
        Assert.Equal("Request to change date of birth case resolved", changes[0].GetElementByTestId("timeline-item-title")!.TextContent.Trim());
        Assert.Equal("by Test User", changes[0].GetElementByTestId("timeline-item-user")!.TextContent.Trim());
        Assert.Null(changes[0].GetElementByTestId("timeline-item-status"));
        Assert.NotNull(changes[0].GetElementByTestId("timeline-item-time"));
        Assert.Equal("Approved", changes[0].GetElementByTestId("timeline-item-summary")!.TextContent.Trim());
        Assert.Equal("Request to change name case resolved", changes[1].GetElementByTestId("timeline-item-title")!.TextContent.Trim());
        Assert.Equal("by Test User", changes[1].GetElementByTestId("timeline-item-user")!.TextContent.Trim());
        Assert.Null(changes[1].GetElementByTestId("timeline-item-status"));
        Assert.NotNull(changes[1].GetElementByTestId("timeline-item-time"));
        Assert.Equal("Rejected", changes[1].GetElementByTestId("timeline-item-summary")!.TextContent.Trim());
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithDeletedMandatoryQualification_DisplaysChangesAsExpected()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification().WithMandatoryQualification());
        var mqs = new (bool RaisedByDqtUser, TestData.MandatoryQualificationInfo Mq, DateTime CreatedUtc)[]
        {
            (true, person.MandatoryQualifications.First(), Clock.UtcNow.AddSeconds(-2)),
            (false, person.MandatoryQualifications.Last(), Clock.UtcNow)
        };

        var dqtUserId = await TestData.GetCurrentCrmUserId();
        var user = await TestData.CreateUser();

        var deletedEvents = new List<MandatoryQualificationDeletedEvent>();

        foreach (var mqInfo in mqs)
        {
            var mq = mqInfo.Mq;
            var establishment = mq.DqtMqEstablishmentValue is string establishmentValue ?
                await TestData.ReferenceDataCache.GetMqEstablishmentByValue(mq.DqtMqEstablishmentValue) :
                null;
            Core.DataStore.Postgres.Models.MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(establishment, out var provider);

            var deletedEvent = new MandatoryQualificationDeletedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = mqInfo.CreatedUtc,
                RaisedBy = mqInfo.RaisedByDqtUser ? RaisedByUserInfo.FromDqtUser(dqtUserId, "Test User") : RaisedByUserInfo.FromUserId(user.UserId),
                PersonId = person.ContactId,
                MandatoryQualification = new()
                {
                    QualificationId = mq.QualificationId,
                    Provider = new()
                    {
                        MandatoryQualificationProviderId = provider?.MandatoryQualificationProviderId,
                        Name = provider?.Name,
                        DqtMqEstablishmentId = establishment?.Id,
                        DqtMqEstablishmentName = establishment?.dfeta_name
                    },
                    Specialism = mq.Specialism,
                    Status = mq.Status,
                    StartDate = mq.StartDate,
                    EndDate = mq.EndDate,
                },
                DeletionReason = "Added in error",
                DeletionReasonDetail = "Some extra information",
                EvidenceFile = null
            };

            deletedEvents.Add(deletedEvent);

            await TestData.DeleteMandatoryQualification(
                mq.QualificationId,
                deletedEvent,
                syncEnabled: true);
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/changelog");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var changes = doc.GetAllElementsByTestId("timeline-item");
        Assert.NotEmpty(changes);
        Assert.Equal(2, changes.Count);
        Assert.Null(changes[0].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By {user.Name} on", changes[0].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.NotNull(changes[0].GetElementByTestId("timeline-item-time"));
        Assert.Equal(deletedEvents[1].DeletionReason, changes[0].GetElementByTestId("deletion-reason")!.TextContent.Trim());
        Assert.Equal(deletedEvents[1].DeletionReasonDetail, changes[0].GetElementByTestId("deletion-reason-detail")!.TextContent.Trim());
        Assert.Equal(deletedEvents[1].MandatoryQualification.Provider!.Name, changes[0].GetElementByTestId("provider")!.TextContent.Trim());
        Assert.Equal(deletedEvents[1].MandatoryQualification.Specialism!.Value.GetTitle(), changes[0].GetElementByTestId("specialism")!.TextContent.Trim());
        Assert.Equal(deletedEvents[1].MandatoryQualification.StartDate!.Value.ToString("d MMMM yyyy"), changes[0].GetElementByTestId("start-date")!.TextContent.Trim());
        Assert.Equal(deletedEvents[1].MandatoryQualification.Status!.Value.GetTitle(), changes[0].GetElementByTestId("status")!.TextContent.Trim());
        Assert.Equal(deletedEvents[1].MandatoryQualification.EndDate.HasValue ? deletedEvents[1].MandatoryQualification.EndDate!.Value.ToString("d MMMM yyyy") : "None", changes[0].GetElementByTestId("end-date")!.TextContent.Trim());

        Assert.Null(changes[1].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By Test User on", changes[1].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.NotNull(changes[0].GetElementByTestId("timeline-item-time"));
        Assert.Equal(deletedEvents[0].DeletionReason, changes[1].GetElementByTestId("deletion-reason")!.TextContent.Trim());
        Assert.Equal(deletedEvents[0].DeletionReasonDetail, changes[1].GetElementByTestId("deletion-reason-detail")!.TextContent.Trim());
        Assert.Equal(deletedEvents[0].MandatoryQualification.Provider!.Name, changes[1].GetElementByTestId("provider")!.TextContent.Trim());
        Assert.Equal(deletedEvents[0].MandatoryQualification.Specialism!.Value.GetTitle(), changes[1].GetElementByTestId("specialism")!.TextContent.Trim());
        Assert.Equal(deletedEvents[0].MandatoryQualification.StartDate!.Value.ToString("d MMMM yyyy"), changes[1].GetElementByTestId("start-date")!.TextContent.Trim());
        Assert.Equal(deletedEvents[0].MandatoryQualification.Status!.Value.GetTitle(), changes[1].GetElementByTestId("status")!.TextContent.Trim());
        Assert.Equal(deletedEvents[0].MandatoryQualification.EndDate.HasValue ? deletedEvents[0].MandatoryQualification.EndDate!.Value.ToString("d MMMM yyyy") : "None", changes[1].GetElementByTestId("end-date")!.TextContent.Trim());
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithDqtDeactivatedMandatoryQualification_DisplaysChangesAsExpected()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification().WithMandatoryQualification());
        var dateTimeOutsideBst = new DateTime(2021, 1, 1, 10, 30, 0, DateTimeKind.Utc);
        var dateTimeInsideBst = new DateTime(2021, 6, 1, 10, 30, 0, DateTimeKind.Utc);
        var mqs = new (bool RaisedByDqtUser, TestData.MandatoryQualificationInfo Mq, DateTime CreatedUtc)[]
        {
            (true, person.MandatoryQualifications.First(), dateTimeOutsideBst),
            (false, person.MandatoryQualifications.Last(), dateTimeInsideBst)
        };

        var dqtUserId = await TestData.GetCurrentCrmUserId();
        var user = await TestData.CreateUser();

        var deactivatedEvents = new List<MandatoryQualificationDqtDeactivatedEvent>();

        await WithDbContext(async dbContext =>
        {
            foreach (var mqInfo in mqs)
            {
                var mq = mqInfo.Mq;
                var establishment = mq.DqtMqEstablishmentValue is string establishmentValue ?
                    await TestData.ReferenceDataCache.GetMqEstablishmentByValue(mq.DqtMqEstablishmentValue) :
                    null;
                Core.DataStore.Postgres.Models.MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(establishment, out var provider);

                var deactivatedEvent = new MandatoryQualificationDqtDeactivatedEvent()
                {
                    EventId = Guid.NewGuid(),
                    CreatedUtc = mqInfo.CreatedUtc,
                    RaisedBy = mqInfo.RaisedByDqtUser ? RaisedByUserInfo.FromDqtUser(dqtUserId, "Test User") : RaisedByUserInfo.FromUserId(user.UserId),
                    PersonId = person.ContactId,
                    MandatoryQualification = new()
                    {
                        QualificationId = mq.QualificationId,
                        Provider = new()
                        {
                            MandatoryQualificationProviderId = provider?.MandatoryQualificationProviderId,
                            Name = provider?.Name,
                            DqtMqEstablishmentId = establishment?.Id,
                            DqtMqEstablishmentName = establishment?.dfeta_name
                        },
                        Specialism = mq.Specialism,
                        Status = mq.Status,
                        StartDate = mq.StartDate,
                        EndDate = mq.EndDate,
                    }
                };

                deactivatedEvents.Add(deactivatedEvent);
                dbContext.AddEvent(deactivatedEvent);
            }

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/changelog");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var changes = doc.GetAllElementsByTestId("timeline-item");
        Assert.NotEmpty(changes);
        Assert.Equal(2, changes.Count);
        Assert.Null(changes[0].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By {user.Name} on", changes[0].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.Equal("01 June 2021 at 11:30 AM", changes[0].GetElementByTestId("timeline-item-time")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[1].MandatoryQualification.Provider!.Name, changes[0].GetElementByTestId("provider")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[1].MandatoryQualification.Specialism!.Value.GetTitle(), changes[0].GetElementByTestId("specialism")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[1].MandatoryQualification.StartDate!.Value.ToString("d MMMM yyyy"), changes[0].GetElementByTestId("start-date")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[1].MandatoryQualification.Status!.Value.GetTitle(), changes[0].GetElementByTestId("status")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[1].MandatoryQualification.EndDate.HasValue ? deactivatedEvents[1].MandatoryQualification.EndDate!.Value.ToString("d MMMM yyyy") : "None", changes[0].GetElementByTestId("end-date")!.TextContent.Trim());

        Assert.Null(changes[1].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By Test User on", changes[1].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.Equal("01 January 2021 at 10:30 AM", changes[1].GetElementByTestId("timeline-item-time")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[0].MandatoryQualification.Provider!.Name, changes[1].GetElementByTestId("provider")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[0].MandatoryQualification.Specialism!.Value.GetTitle(), changes[1].GetElementByTestId("specialism")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[0].MandatoryQualification.StartDate!.Value.ToString("d MMMM yyyy"), changes[1].GetElementByTestId("start-date")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[0].MandatoryQualification.Status!.Value.GetTitle(), changes[1].GetElementByTestId("status")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[0].MandatoryQualification.EndDate.HasValue ? deactivatedEvents[0].MandatoryQualification.EndDate!.Value.ToString("d MMMM yyyy") : "None", changes[1].GetElementByTestId("end-date")!.TextContent.Trim());
    }
}
