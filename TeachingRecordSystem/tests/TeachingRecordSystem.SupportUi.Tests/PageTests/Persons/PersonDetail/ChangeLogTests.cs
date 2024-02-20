using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.Core.Events.Models;
using File = TeachingRecordSystem.Core.Events.Models.File;

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{nonExistentPersonId}/change-history");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
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
                        MandatoryQualificationProviderId = mqInfo.RaisedByDqtUser ? null : provider?.MandatoryQualificationProviderId,
                        Name = mqInfo.RaisedByDqtUser ? null : provider?.Name,
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var changes = doc.GetAllElementsByTestId("timeline-item-mq-deleted-event");
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
        Assert.Equal(deletedEvents[0].MandatoryQualification.Provider!.DqtMqEstablishmentName, changes[1].GetElementByTestId("provider")!.TextContent.Trim());
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
                            MandatoryQualificationProviderId = mqInfo.RaisedByDqtUser ? null : provider?.MandatoryQualificationProviderId,
                            Name = mqInfo.RaisedByDqtUser ? null : provider?.Name,
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var changes = doc.GetAllElementsByTestId("timeline-item-mq-dqt-deactivated-event");
        Assert.NotEmpty(changes);
        Assert.Equal(2, changes.Count);
        Assert.Null(changes[0].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By {user.Name} on", changes[0].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.Equal("1 June 2021 at 11:30 AM", changes[0].GetElementByTestId("timeline-item-time")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[1].MandatoryQualification.Provider!.Name, changes[0].GetElementByTestId("provider")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[1].MandatoryQualification.Specialism!.Value.GetTitle(), changes[0].GetElementByTestId("specialism")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[1].MandatoryQualification.StartDate!.Value.ToString("d MMMM yyyy"), changes[0].GetElementByTestId("start-date")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[1].MandatoryQualification.Status!.Value.GetTitle(), changes[0].GetElementByTestId("status")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[1].MandatoryQualification.EndDate.HasValue ? deactivatedEvents[1].MandatoryQualification.EndDate!.Value.ToString("d MMMM yyyy") : "None", changes[0].GetElementByTestId("end-date")!.TextContent.Trim());

        Assert.Null(changes[1].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By Test User on", changes[1].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.Equal("1 January 2021 at 10:30 AM", changes[1].GetElementByTestId("timeline-item-time")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[0].MandatoryQualification.Provider!.DqtMqEstablishmentName, changes[1].GetElementByTestId("provider")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[0].MandatoryQualification.Specialism!.Value.GetTitle(), changes[1].GetElementByTestId("specialism")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[0].MandatoryQualification.StartDate!.Value.ToString("d MMMM yyyy"), changes[1].GetElementByTestId("start-date")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[0].MandatoryQualification.Status!.Value.GetTitle(), changes[1].GetElementByTestId("status")!.TextContent.Trim());
        Assert.Equal(deactivatedEvents[0].MandatoryQualification.EndDate.HasValue ? deactivatedEvents[0].MandatoryQualification.EndDate!.Value.ToString("d MMMM yyyy") : "None", changes[1].GetElementByTestId("end-date")!.TextContent.Trim());
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithDqtReactivatedMandatoryQualification_DisplaysChangesAsExpected()
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

        var reactivatedEvents = new List<MandatoryQualificationDqtReactivatedEvent>();

        await WithDbContext(async dbContext =>
        {
            foreach (var mqInfo in mqs)
            {
                var mq = mqInfo.Mq;
                var establishment = mq.DqtMqEstablishmentValue is string establishmentValue ?
                    await TestData.ReferenceDataCache.GetMqEstablishmentByValue(mq.DqtMqEstablishmentValue) :
                    null;
                Core.DataStore.Postgres.Models.MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(establishment, out var provider);

                var reactivatedEvent = new MandatoryQualificationDqtReactivatedEvent()
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

                reactivatedEvents.Add(reactivatedEvent);
                dbContext.AddEvent(reactivatedEvent);
            }

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var changes = doc.GetAllElementsByTestId("timeline-item-mq-dqt-reactivated-event");
        Assert.NotEmpty(changes);
        Assert.Equal(2, changes.Count);
        Assert.Null(changes[0].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By {user.Name} on", changes[0].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.Equal("1 June 2021 at 11:30 AM", changes[0].GetElementByTestId("timeline-item-time")!.TextContent.Trim());
        Assert.Null(changes[1].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By Test User on", changes[1].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.Equal("1 January 2021 at 10:30 AM", changes[1].GetElementByTestId("timeline-item-time")!.TextContent.Trim());
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithDqtImportedMandatoryQualification_DisplaysChangesAsExpected()
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

        var reactivatedEvents = new List<MandatoryQualificationDqtImportedEvent>();

        await WithDbContext(async dbContext =>
        {
            foreach (var mqInfo in mqs)
            {
                var mq = mqInfo.Mq;
                var establishment = mq.DqtMqEstablishmentValue is string establishmentValue ?
                    await TestData.ReferenceDataCache.GetMqEstablishmentByValue(mq.DqtMqEstablishmentValue) :
                    null;
                Core.DataStore.Postgres.Models.MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(establishment, out var provider);

                var reactivatedEvent = new MandatoryQualificationDqtImportedEvent()
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
                    DqtState = 0
                };

                reactivatedEvents.Add(reactivatedEvent);
                dbContext.AddEvent(reactivatedEvent);
            }

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var changes = doc.GetAllElementsByTestId("timeline-item-mq-dqt-imported-event");
        Assert.NotEmpty(changes);
        Assert.Equal(2, changes.Count);
        Assert.Null(changes[0].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By {user.Name} on", changes[0].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.Equal("1 June 2021 at 11:30 AM", changes[0].GetElementByTestId("timeline-item-time")!.TextContent.Trim());
        Assert.Null(changes[1].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By Test User on", changes[1].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.Equal("1 January 2021 at 10:30 AM", changes[1].GetElementByTestId("timeline-item-time")!.TextContent.Trim());
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithCreatedMandatoryQualification_DisplaysChangesAsExpected()
    {
        // Arrange
        var dqtUserId = await TestData.GetCurrentCrmUserId();
        var user = await TestData.CreateUser();
        var dateTimeOutsideBst = new DateTime(2021, 1, 1, 10, 30, 0, DateTimeKind.Utc);
        var dateTimeInsideBst = new DateTime(2021, 6, 1, 10, 30, 0, DateTimeKind.Utc);
        var person = await TestData.CreatePerson(b => b
            .WithMandatoryQualification(b => b.WithCreatedUtc(dateTimeOutsideBst).WithCreatedByUser(RaisedByUserInfo.FromDqtUser(dqtUserId, "Test User")))
            .WithMandatoryQualification(b => b.WithCreatedUtc(dateTimeInsideBst).WithCreatedByUser(RaisedByUserInfo.FromUserId(user.UserId))));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var changes = doc.GetAllElementsByTestId("timeline-item-mq-created-event");
        Assert.NotEmpty(changes);
        Assert.Equal(2, changes.Count);
        Assert.Null(changes[0].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By {user.Name} on", changes[0].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.Equal("1 June 2021 at 11:30 AM", changes[0].GetElementByTestId("timeline-item-time")!.TextContent.Trim());
        Assert.Null(changes[1].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By Test User on", changes[1].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.Equal("1 January 2021 at 10:30 AM", changes[1].GetElementByTestId("timeline-item-time")!.TextContent.Trim());
    }

    [Theory]
    [InlineData(MandatoryQualificationUpdatedEventChanges.Provider)]
    [InlineData(MandatoryQualificationUpdatedEventChanges.Specialism)]
    [InlineData(MandatoryQualificationUpdatedEventChanges.Status)]
    [InlineData(MandatoryQualificationUpdatedEventChanges.StartDate)]
    [InlineData(MandatoryQualificationUpdatedEventChanges.EndDate)]
    [InlineData(MandatoryQualificationUpdatedEventChanges.Provider | MandatoryQualificationUpdatedEventChanges.Specialism | MandatoryQualificationUpdatedEventChanges.Status | MandatoryQualificationUpdatedEventChanges.StartDate | MandatoryQualificationUpdatedEventChanges.EndDate)]
    public async Task Get_WithPersonIdForPersonWithUpdatedMandatoryQualification_DisplaysChangesAsExpected(MandatoryQualificationUpdatedEventChanges changes)
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(b => b.WithStatus(MandatoryQualificationStatus.InProgress)).WithMandatoryQualification(b => b.WithStatus(MandatoryQualificationStatus.InProgress)));
        var dateTimeOutsideBst = new DateTime(2021, 1, 1, 10, 30, 0, DateTimeKind.Utc);
        var dateTimeInsideBst = new DateTime(2021, 6, 1, 10, 30, 0, DateTimeKind.Utc);
        var mqs = new (bool RaisedByDqtUser, TestData.MandatoryQualificationInfo Mq, DateTime CreatedUtc, bool HasEvidence)[]
        {
            (true, person.MandatoryQualifications.First(), dateTimeOutsideBst, true),
            (false, person.MandatoryQualifications.Last(), dateTimeInsideBst, false)
        };

        var dqtUserId = await TestData.GetCurrentCrmUserId();
        var user = await TestData.CreateUser();

        var updatedEvents = new List<MandatoryQualificationUpdatedEvent>();
        var mqEstablishments = await TestData.ReferenceDataCache.GetMqEstablishments();

        await WithDbContext(async dbContext =>
        {
            foreach (var mqInfo in mqs)
            {
                var mq = mqInfo.Mq;
                var establishment = mq.DqtMqEstablishmentValue is string establishmentValue ?
                    await TestData.ReferenceDataCache.GetMqEstablishmentByValue(mq.DqtMqEstablishmentValue) :
                    null;

                var newEstablishment = establishment;
                var newSpecialism = mq.Specialism;
                var newStartDate = mq.StartDate;
                var newStatus = mq.Status;
                var newEndDate = mq.EndDate;
                string? changeReason = null;
                string? changeReasonDetail = "More detail about the reason for change";
                File? evidenceFile = mqInfo.HasEvidence ? new File() { FileId = Guid.NewGuid(), Name = "MyEvidence.jpg" } : null;
                int changeCount = 0;

                if (changes.HasFlag(MandatoryQualificationUpdatedEventChanges.Provider))
                {
                    newEstablishment = mqEstablishments.RandomOne();
                    changeReason = "Change of training provider";
                    changeCount++;
                }

                if (changes.HasFlag(MandatoryQualificationUpdatedEventChanges.Specialism))
                {
                    newSpecialism = MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: true).RandomOne().Value;
                    changeReason = "Change of specialism";
                    changeCount++;
                }

                if (changes.HasFlag(MandatoryQualificationUpdatedEventChanges.StartDate))
                {
                    newStartDate = TestData.GenerateDate(min: new DateOnly(2000, 1, 1));
                    changeReason = "Change of start date";
                    changeCount++;
                }

                if (changes.HasFlag(MandatoryQualificationUpdatedEventChanges.Status) && changes.HasFlag(MandatoryQualificationUpdatedEventChanges.EndDate))
                {
                    newStatus = MandatoryQualificationStatus.Passed;
                    newEndDate = newStartDate!.Value.AddDays(90);
                    changeReason = "Change of status";
                    changeCount++;
                }
                else if (changes.HasFlag(MandatoryQualificationUpdatedEventChanges.Status))
                {
                    newStatus = MandatoryQualificationStatus.Failed;
                    changeReason = "Change of status";
                    changeCount++;
                }
                else if (changes.HasFlag(MandatoryQualificationUpdatedEventChanges.EndDate))
                {
                    newEndDate = newStartDate!.Value.AddDays(90);
                    changeReason = "Change of end date";
                    changeCount++;
                }

                if (changeCount > 1)
                {
                    // Can only have multiple changes from legacy CRM (other than status and end date for Passed in TRS)
                    changeReason = null;
                    changeReasonDetail = null;
                    evidenceFile = null;
                }

                Core.DataStore.Postgres.Models.MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(establishment, out var provider);

                var updatedEvent = new MandatoryQualificationUpdatedEvent()
                {
                    EventId = Guid.NewGuid(),
                    CreatedUtc = mqInfo.CreatedUtc,
                    RaisedBy = mqInfo.RaisedByDqtUser ? RaisedByUserInfo.FromDqtUser(dqtUserId, "Test User") : RaisedByUserInfo.FromUserId(user.UserId),
                    PersonId = person.ContactId,
                    OldMandatoryQualification = new MandatoryQualification()
                    {
                        QualificationId = mq.QualificationId,
                        Provider = new()
                        {
                            MandatoryQualificationProviderId = mqInfo.RaisedByDqtUser ? null : provider?.MandatoryQualificationProviderId,
                            Name = mqInfo.RaisedByDqtUser ? null : provider?.Name,
                            DqtMqEstablishmentId = establishment?.Id,
                            DqtMqEstablishmentName = establishment?.dfeta_name
                        },
                        Specialism = mq.Specialism,
                        Status = mq.Status,
                        StartDate = mq.StartDate,
                        EndDate = mq.EndDate,
                    },
                    MandatoryQualification = new MandatoryQualification()
                    {
                        QualificationId = mq.QualificationId,
                        Provider = new()
                        {
                            MandatoryQualificationProviderId = null,
                            Name = null,
                            DqtMqEstablishmentId = newEstablishment?.Id,
                            DqtMqEstablishmentName = newEstablishment?.dfeta_name
                        },
                        Specialism = newSpecialism,
                        Status = newStatus,
                        StartDate = newStartDate,
                        EndDate = newEndDate,
                    },
                    ChangeReason = changeReason,
                    ChangeReasonDetail = changeReasonDetail,
                    EvidenceFile = evidenceFile,
                    Changes = changes
                };

                updatedEvents.Add(updatedEvent);
                dbContext.AddEvent(updatedEvent);
            }

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var timelineItems = doc.GetAllElementsByTestId("timeline-item-mq-updated-event");
        Assert.NotEmpty(timelineItems);
        Assert.Equal(2, timelineItems.Count);
        Assert.Null(timelineItems[0].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By {user.Name} on", timelineItems[0].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.Equal("1 June 2021 at 11:30 AM", timelineItems[0].GetElementByTestId("timeline-item-time")!.TextContent.Trim());
        var reasonForChange1 = timelineItems[0].GetElementByTestId("reason-for-change");
        if (updatedEvents[1].ChangeReason is null)
        {
            Assert.Null(reasonForChange1);
        }
        else
        {
            Assert.NotNull(reasonForChange1);
            Assert.Equal(updatedEvents[1].ChangeReason, reasonForChange1.GetElementByTestId("change-reason")!.TextContent.Trim());
            Assert.Equal(!string.IsNullOrEmpty(updatedEvents[1].ChangeReasonDetail) ? updatedEvents[1].ChangeReasonDetail : "None", reasonForChange1.GetElementByTestId("change-reason-detail")!.TextContent.Trim());
            var evidenceFile = reasonForChange1.GetElementByTestId("evidence");
            if (updatedEvents[1].EvidenceFile is null)
            {
                Assert.Null(evidenceFile);
            }
            else
            {
                Assert.NotNull(evidenceFile);
            }
        }

        var previousData1 = timelineItems[0].GetElementByTestId("previous-data");
        Assert.NotNull(previousData1);
        var oldMq1 = updatedEvents[1].OldMandatoryQualification;
        if (updatedEvents[1].Changes.HasFlag(MandatoryQualificationUpdatedEventChanges.Provider))
        {
            Assert.Equal(oldMq1.Provider is not null ? oldMq1.Provider!.Name : "None", previousData1.GetElementByTestId("provider")!.TextContent.Trim());
        }
        else
        {
            Assert.Null(previousData1.GetElementByTestId("provider"));
        }

        if (updatedEvents[1].Changes.HasFlag(MandatoryQualificationUpdatedEventChanges.Specialism))
        {
            Assert.Equal(oldMq1.Specialism.HasValue ? oldMq1.Specialism!.Value.GetTitle() : "None", previousData1.GetElementByTestId("specialism")!.TextContent.Trim());
        }
        else
        {
            Assert.Null(previousData1.GetElementByTestId("specialism"));
        }

        if (updatedEvents[1].Changes.HasFlag(MandatoryQualificationUpdatedEventChanges.StartDate))
        {
            Assert.Equal(oldMq1.StartDate.HasValue ? oldMq1.StartDate!.Value.ToString("d MMMM yyyy") : "None", previousData1.GetElementByTestId("start-date")!.TextContent.Trim());
        }
        else
        {
            Assert.Null(previousData1.GetElementByTestId("start-date"));
        }

        if (updatedEvents[1].Changes.HasFlag(MandatoryQualificationUpdatedEventChanges.Status))
        {
            Assert.Equal(oldMq1.Status.HasValue ? oldMq1.Status!.Value.GetTitle() : "None", previousData1.GetElementByTestId("status")!.TextContent.Trim());
        }
        else
        {
            Assert.Null(previousData1.GetElementByTestId("status"));
        }

        if (updatedEvents[1].Changes.HasFlag(MandatoryQualificationUpdatedEventChanges.EndDate))
        {
            Assert.Equal(oldMq1.EndDate.HasValue ? oldMq1.EndDate!.Value.ToString("d MMMM yyyy") : "None", previousData1.GetElementByTestId("end-date")!.TextContent.Trim());
        }
        else
        {
            Assert.Null(previousData1.GetElementByTestId("end-date"));
        }

        Assert.Null(timelineItems[1].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By Test User on", timelineItems[1].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.Equal("1 January 2021 at 10:30 AM", timelineItems[1].GetElementByTestId("timeline-item-time")!.TextContent.Trim());

        var reasonForChange2 = timelineItems[1].GetElementByTestId("reason-for-change");
        if (updatedEvents[0].ChangeReason is null)
        {
            Assert.Null(reasonForChange2);
        }
        else
        {
            Assert.NotNull(reasonForChange2);
            Assert.Equal(updatedEvents[0].ChangeReason, reasonForChange2.GetElementByTestId("change-reason")!.TextContent.Trim());
            Assert.Equal(!string.IsNullOrEmpty(updatedEvents[0].ChangeReasonDetail) ? updatedEvents[0].ChangeReasonDetail : "None", reasonForChange2.GetElementByTestId("change-reason-detail")!.TextContent.Trim());
            var evidenceFile = reasonForChange2.GetElementByTestId("evidence");
            if (updatedEvents[0].EvidenceFile is null)
            {
                Assert.Null(evidenceFile);
            }
            else
            {
                Assert.NotNull(evidenceFile);
            }
        }

        var previousData2 = timelineItems[1].GetElementByTestId("previous-data");
        Assert.NotNull(previousData2);
        var oldMq2 = updatedEvents[0].OldMandatoryQualification;
        if (updatedEvents[0].Changes.HasFlag(MandatoryQualificationUpdatedEventChanges.Provider))
        {
            Assert.Equal(oldMq2.Provider is not null ? oldMq2.Provider!.DqtMqEstablishmentName : "None", previousData2.GetElementByTestId("provider")!.TextContent.Trim());
        }
        else
        {
            Assert.Null(previousData2.GetElementByTestId("provider"));
        }

        if (updatedEvents[0].Changes.HasFlag(MandatoryQualificationUpdatedEventChanges.Specialism))
        {
            Assert.Equal(oldMq2.Specialism.HasValue ? oldMq2.Specialism!.Value.GetTitle() : "None", previousData2.GetElementByTestId("specialism")!.TextContent.Trim());
        }
        else
        {
            Assert.Null(previousData2.GetElementByTestId("specialism"));
        }

        if (updatedEvents[0].Changes.HasFlag(MandatoryQualificationUpdatedEventChanges.StartDate))
        {
            Assert.Equal(oldMq2.StartDate.HasValue ? oldMq2.StartDate!.Value.ToString("d MMMM yyyy") : "None", previousData2.GetElementByTestId("start-date")!.TextContent.Trim());
        }
        else
        {
            Assert.Null(previousData2.GetElementByTestId("start-date"));
        }

        if (updatedEvents[0].Changes.HasFlag(MandatoryQualificationUpdatedEventChanges.Status))
        {
            Assert.Equal(oldMq2.Status.HasValue ? oldMq2.Status!.Value.GetTitle() : "None", previousData2.GetElementByTestId("status")!.TextContent.Trim());
        }
        else
        {
            Assert.Null(previousData2.GetElementByTestId("status"));
        }

        if (updatedEvents[0].Changes.HasFlag(MandatoryQualificationUpdatedEventChanges.EndDate))
        {
            Assert.Equal(oldMq2.EndDate.HasValue ? oldMq2.EndDate!.Value.ToString("d MMMM yyyy") : "None", previousData2.GetElementByTestId("end-date")!.TextContent.Trim());
        }
        else
        {
            Assert.Null(previousData2.GetElementByTestId("end-date"));
        }
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithMigratedMandatoryQualification_DisplaysChangesAsExpected()
    {
        // Arrange
        var establishmentWhichNeedsMigrating = "210"; // Postgraduate Diploma in Multi-Sensory Impairment and Deafblindness, University of Birmingham, School of Education
        var establishmentWhichDoesNotNeedMigrating = "955"; // University of Birmingham
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(b => b.WithDqtMqEstablishmentValue(establishmentWhichNeedsMigrating)).WithMandatoryQualification(b => b.WithDqtMqEstablishmentValue(establishmentWhichDoesNotNeedMigrating)));
        var dateTimeOutsideBst = new DateTime(2021, 1, 1, 10, 30, 0, DateTimeKind.Utc);
        var dateTimeInsideBst = new DateTime(2021, 6, 1, 10, 30, 0, DateTimeKind.Utc);
        var mqs = new (bool RaisedByDqtUser, TestData.MandatoryQualificationInfo Mq, DateTime CreatedUtc)[]
        {
            (true, person.MandatoryQualifications.First(), dateTimeOutsideBst),
            (false, person.MandatoryQualifications.Last(), dateTimeInsideBst)
        };

        var dqtUserId = await TestData.GetCurrentCrmUserId();
        var user = await TestData.CreateUser();

        var migratedEvents = new List<MandatoryQualificationMigratedEvent>();

        await WithDbContext(async dbContext =>
        {
            foreach (var mqInfo in mqs)
            {
                var mq = mqInfo.Mq;
                var establishment = mq.DqtMqEstablishmentValue is string establishmentValue ?
                    await TestData.ReferenceDataCache.GetMqEstablishmentByValue(mq.DqtMqEstablishmentValue) :
                    null;
                Core.DataStore.Postgres.Models.MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(establishment, out var provider);

                var changes = (provider?.Name != establishment?.dfeta_name) ? MandatoryQualificationMigratedEventChanges.Provider : MandatoryQualificationMigratedEventChanges.None;

                var migratedEvent = new MandatoryQualificationMigratedEvent()
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
                    Changes = changes
                };

                migratedEvents.Add(migratedEvent);
                dbContext.AddEvent(migratedEvent);
            }

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var timelineItems = doc.GetAllElementsByTestId("timeline-item-mq-migrated-event");
        Assert.NotEmpty(timelineItems);
        Assert.Equal(2, timelineItems.Count);
        Assert.Null(timelineItems[0].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By {user.Name} on", timelineItems[0].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.Equal("1 June 2021 at 11:30 AM", timelineItems[0].GetElementByTestId("timeline-item-time")!.TextContent.Trim());
        var previousData1 = timelineItems[0].GetElementByTestId("previous-data");
        Assert.Null(previousData1);

        Assert.Null(timelineItems[1].GetElementByTestId("timeline-item-status"));
        Assert.Equal($"By Test User on", timelineItems[1].GetElementByTestId("raised-by")!.TextContent.Trim());
        Assert.Equal("1 January 2021 at 10:30 AM", timelineItems[1].GetElementByTestId("timeline-item-time")!.TextContent.Trim());
        var previousData2 = timelineItems[1].GetElementByTestId("previous-data");
        Assert.NotNull(previousData2);
        Assert.Equal(migratedEvents[0].MandatoryQualification.Provider!.DqtMqEstablishmentName, previousData2.GetElementByTestId("provider")!.TextContent.Trim());
    }
}
