using System.Security.Claims;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;
using TeachingRecordSystem.SupportUi.Services.ChangeHistory;
using PaginationOptions = TeachingRecordSystem.SupportUi.Services.PaginationOptions;

namespace TeachingRecordSystem.SupportUi.Tests.Services.ChangeHistory;

[Collection(nameof(DisableParallelization)), ClearDbBeforeTest]
public class ChangeHistoryServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public async Task GetChangeHistoryByPersonAsync_PersonHasNoChanges_ReturnsEmptyResultPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        // Act
        var result = await GetChangeHistoryByPersonAsync(person.PersonId, await CreatePrincipalAsync(), new());

        // Assert
        Assert.Empty(result);
        Assert.Equal(0, result.TotalItemCount);
        Assert.Equal(1, result.CurrentPage);
    }

    [Fact]
    public async Task GetChangeHistoryByPersonAsync_MapsLegacyEventToTimelineItem()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();
        var createdUtc = TimeProvider.UtcNow;

        await CreateNameChangeEventAsync(person.PersonId, "Smith", createdUtc, user.UserId);

        // Act
        var result = await GetChangeHistoryByPersonAsync(person.PersonId, await CreatePrincipalAsync(), new());

        // Assert
        var item = Assert.Single(result);
        Assert.Equal(TimelineItemType.LegacyEvent, item.ItemType);
        Assert.Equal(person.PersonId, item.PersonId);
        Assert.Equal(createdUtc, item.Timestamp);

        var entry = Assert.IsType<LegacyEventChangeHistoryEntry<LegacyEvents.PersonDetailsUpdatedEvent>>(item.ItemModel);
        Assert.Equal(user.Name, entry.RaisedByUser.Name);
        Assert.Equal("Smith", entry.Event.PersonAttributes.LastName);
    }

    [Fact]
    public async Task GetChangeHistoryByPersonAsync_EventRaisedByDqtUser_UsesDqtUserName()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = TimeProvider.UtcNow,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(Guid.NewGuid(), "Some DQT User"),
                PersonId = person.PersonId,
                PersonAttributes = CreatePersonDetails("Smith"),
                OldPersonAttributes = CreatePersonDetails("Jones"),
                NameChangeReason = null,
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.LastName
            });
            await dbContext.SaveChangesAsync();
        });

        // Act
        var result = await GetChangeHistoryByPersonAsync(person.PersonId, await CreatePrincipalAsync(), new());

        // Assert
        var item = Assert.Single(result);
        var entry = Assert.IsType<LegacyEventChangeHistoryEntry<LegacyEvents.PersonDetailsUpdatedEvent>>(item.ItemModel);
        Assert.Equal("Some DQT User", entry.RaisedByUser.Name);
    }

    [Fact]
    public async Task GetChangeHistoryByPersonAsync_DoesNotReturnEventsForOtherPeople()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var otherPerson = await TestData.CreatePersonAsync();

        await CreateNameChangeEventAsync(otherPerson.PersonId, "Smith", TimeProvider.UtcNow);

        // Act
        var result = await GetChangeHistoryByPersonAsync(person.PersonId, await CreatePrincipalAsync(), new());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetChangeHistoryByPersonAsync_ExcludesEventTypesThatAreNotPartOfChangeHistory()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        // QtsAwardedEmailSentEvent has a PersonId but is not one of the event types shown in the change history
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(new LegacyEvents.QtsAwardedEmailSentEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = TimeProvider.UtcNow,
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                QtsAwardedEmailsJobId = Guid.NewGuid(),
                EmailAddress = "test@example.com"
            });
            await dbContext.SaveChangesAsync();
        });

        // Act
        var result = await GetChangeHistoryByPersonAsync(person.PersonId, await CreatePrincipalAsync(), new());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetChangeHistoryByPersonAsync_OrdersItemsByTimestampDescending()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var baseTime = TimeProvider.UtcNow;

        await CreateNameChangeEventAsync(person.PersonId, "Oldest", baseTime.AddMinutes(-10));
        await CreateNameChangeEventAsync(person.PersonId, "Newest", baseTime);
        await CreateNameChangeEventAsync(person.PersonId, "Middle", baseTime.AddMinutes(-5));

        // Act
        var result = await GetChangeHistoryByPersonAsync(person.PersonId, await CreatePrincipalAsync(), new());

        // Assert
        Assert.Equal(["Newest", "Middle", "Oldest"], GetLastNames(result));
    }

    [Fact]
    public async Task GetChangeHistoryByPersonAsync_CombinesLegacyEventsAndProcessesOrderedByTimestamp()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();
        var baseTime = TimeProvider.UtcNow;

        await CreateNameChangeEventAsync(person.PersonId, "Smith", baseTime.AddMinutes(-10));
        // The process is created 'now', so it is newer than the legacy event
        var process = await CreateReactivatingProcessAsync(person.PersonId, user.UserId);

        // Act
        var result = await GetChangeHistoryByPersonAsync(person.PersonId, await CreatePrincipalAsync(), new());

        // Assert
        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal(TimelineItemType.Process, first.ItemType);
                Assert.Equal(process.ProcessId, Assert.IsType<ProcessChangeHistoryEntry>(first.ItemModel).Process.ProcessId);
            },
            second => Assert.Equal(TimelineItemType.LegacyEvent, second.ItemType));
    }

    [Fact]
    public async Task GetChangeHistoryByPersonAsync_MapsProcessToTimelineItem()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var process = await CreateReactivatingProcessAsync(person.PersonId, user.UserId);

        // Act
        var result = await GetChangeHistoryByPersonAsync(person.PersonId, await CreatePrincipalAsync(), new());

        // Assert
        var item = Assert.Single(result);
        Assert.Equal(TimelineItemType.Process, item.ItemType);
        Assert.Equal(person.PersonId, item.PersonId);
        Assert.Equal(process.CreatedOn, item.Timestamp);

        var entry = Assert.IsType<ProcessChangeHistoryEntry>(item.ItemModel);
        Assert.Equal(process.ProcessId, entry.Process.ProcessId);
        Assert.Equal(user.Name, entry.RaisedByUser.Name);
    }

    [Theory]
    [InlineData(1, new[] { "Name0", "Name1" })]
    [InlineData(2, new[] { "Name2", "Name3" })]
    [InlineData(3, new[] { "Name4" })]
    [InlineData(4, new string[0])]
    public async Task GetChangeHistoryByPersonAsync_Pagination(int pageNumber, string[] expectedLastNames)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var baseTime = TimeProvider.UtcNow;

        for (var i = 0; i < 5; i++)
        {
            await CreateNameChangeEventAsync(person.PersonId, $"Name{i}", baseTime.AddMinutes(-i));
        }

        // Act
        var result = await GetChangeHistoryByPersonAsync(
            person.PersonId,
            await CreatePrincipalAsync(),
            new(PageNumber: pageNumber, PageSize: 2));

        // Assert
        Assert.Equal(5, result.TotalItemCount);
        Assert.Equal(pageNumber, result.CurrentPage);
        Assert.Equal(expectedLastNames, GetLastNames(result));
    }

    [Theory]
    [InlineData(false, UserRoles.Viewer, true)]
    [InlineData(false, null, false)]
    [InlineData(true, UserRoles.Viewer, false)]
    [InlineData(true, UserRoles.AlertsManagerTraDbs, true)]
    public async Task GetChangeHistoryByPersonAsync_FiltersDqtAlertProcessesByAlertTypeReadPermission(bool isDbsAlertType, string? role, bool shouldInclude)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var alertType = isDbsAlertType
            ? await TestData.ReferenceDataCache.GetAlertTypeByIdAsync(AlertType.DbsAlertTypeId)
            : (await TestData.ReferenceDataCache.GetAlertTypesAsync()).First(t => !t.IsDbsAlertType);

        await CreateAlertImportingIntoDqtProcessAsync(person.PersonId, alertType.AlertTypeId);

        var principal = await CreatePrincipalAsync(role);

        // Act
        var result = await GetChangeHistoryByPersonAsync(person.PersonId, principal, new());

        // Assert
        if (shouldInclude)
        {
            var item = Assert.Single(result);
            Assert.Equal(TimelineItemType.Process, item.ItemType);
        }
        else
        {
            Assert.Empty(result);
        }
    }

    [Theory]
    [InlineData(false, UserRoles.Viewer, true)]
    [InlineData(false, null, false)]
    [InlineData(true, UserRoles.Viewer, false)]
    [InlineData(true, UserRoles.AlertsManagerTraDbs, true)]
    public async Task GetChangeHistoryByPersonAsync_FiltersAlertProcessesByAlertTypeReadPermission(bool isDbsAlertType, string? role, bool shouldInclude)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();
        var alertType = isDbsAlertType
            ? await TestData.ReferenceDataCache.GetAlertTypeByIdAsync(AlertType.DbsAlertTypeId)
            : (await TestData.ReferenceDataCache.GetAlertTypesAsync()).First(t => !t.IsDbsAlertType);

        await CreateAlertCreatingProcessAsync(person.PersonId, alertType.AlertTypeId, user.UserId);

        var principal = await CreatePrincipalAsync(role);

        // Act
        var result = await GetChangeHistoryByPersonAsync(person.PersonId, principal, new());

        // Assert
        if (shouldInclude)
        {
            var item = Assert.Single(result);
            Assert.Equal(TimelineItemType.Process, item.ItemType);
        }
        else
        {
            Assert.Empty(result);
        }
    }

    [Fact]
    public async Task GetChangeHistoryBySupportTaskAsync_SupportTaskHasNoProcesses_ReturnsEmptyCollection()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync();

        // Act
        var result = await GetChangeHistoryBySupportTaskAsync(supportTask.SupportTaskReference);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetChangeHistoryBySupportTaskAsync_MapsProcessToEntry()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync();
        var user = await TestData.CreateUserAsync();

        var process = await CreateNoteCreatingProcessAsync(supportTask.SupportTaskReference, user.UserId, "Some note");

        // Act
        var result = await GetChangeHistoryBySupportTaskAsync(supportTask.SupportTaskReference);

        // Assert
        var entry = Assert.Single(result);
        Assert.Equal(process.ProcessId, entry.Process.ProcessId);
        Assert.Equal(ProcessType.SupportTaskNoteCreating, entry.Process.ProcessType);
        Assert.Equal(user.Name, entry.RaisedByUser.Name);
        Assert.Equal("Some note", entry.GetEvent<SupportTaskNoteCreatedEvent>().SupportTaskNote.Content);
    }

    [Fact]
    public async Task GetChangeHistoryBySupportTaskAsync_ProcessRaisedByDqtUser_UsesDqtUserName()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync();

        await CreateNoteCreatingProcessAsync(supportTask.SupportTaskReference, userId: null, dqtUserName: "Some DQT User");

        // Act
        var result = await GetChangeHistoryBySupportTaskAsync(supportTask.SupportTaskReference);

        // Assert
        var entry = Assert.Single(result);
        Assert.Equal("Some DQT User", entry.RaisedByUser.Name);
    }

    [Fact]
    public async Task GetChangeHistoryBySupportTaskAsync_DoesNotReturnProcessesForOtherSupportTasks()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync();
        var otherSupportTask = await CreateSupportTaskAsync();
        var user = await TestData.CreateUserAsync();

        await CreateNoteCreatingProcessAsync(otherSupportTask.SupportTaskReference, user.UserId);

        // Act
        var result = await GetChangeHistoryBySupportTaskAsync(supportTask.SupportTaskReference);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetChangeHistoryBySupportTaskAsync_ReturnsEveryProcessForSupportTask()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync();
        var user = await TestData.CreateUserAsync();

        var processIds = new List<Guid>();
        for (var i = 0; i < 3; i++)
        {
            processIds.Add((await CreateNoteCreatingProcessAsync(supportTask.SupportTaskReference, user.UserId)).ProcessId);
            TimeProvider.Advance();
        }

        // Act
        var result = await GetChangeHistoryBySupportTaskAsync(supportTask.SupportTaskReference);

        // Assert
        Assert.Equal(
            processIds.OrderBy(id => id),
            result.Select(e => e.Process.ProcessId).OrderBy(id => id));
    }

    [Fact]
    public async Task GetChangeHistoryBySupportTaskAsync_ProcessReferencesMultipleSupportTasks_IsReturnedForEachSupportTask()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync();
        var otherSupportTask = await CreateSupportTaskAsync();
        var user = await TestData.CreateUserAsync();

        var process = await CreateAssigningProcessAsync(user.UserId, supportTask, otherSupportTask);

        // Act
        var result = await GetChangeHistoryBySupportTaskAsync(supportTask.SupportTaskReference);
        var otherResult = await GetChangeHistoryBySupportTaskAsync(otherSupportTask.SupportTaskReference);

        // Assert
        Assert.Equal(process.ProcessId, Assert.Single(result).Process.ProcessId);
        Assert.Equal(process.ProcessId, Assert.Single(otherResult).Process.ProcessId);
    }

    [Fact]
    public async Task GetChangeHistoryBySupportTaskAsync_OrdersProcessesByCreatedOnDescending()
    {
        // Arrange
        var supportTask = await CreateSupportTaskAsync();
        var user = await TestData.CreateUserAsync();

        await CreateNoteCreatingProcessAsync(supportTask.SupportTaskReference, user.UserId, "Oldest");
        TimeProvider.Advance();
        await CreateNoteCreatingProcessAsync(supportTask.SupportTaskReference, user.UserId, "Middle");
        TimeProvider.Advance();
        await CreateNoteCreatingProcessAsync(supportTask.SupportTaskReference, user.UserId, "Newest");

        // Act
        var result = await GetChangeHistoryBySupportTaskAsync(supportTask.SupportTaskReference);

        // Assert
        Assert.Equal(["Newest", "Middle", "Oldest"], GetNoteContents(result));
    }

    private Task<ResultPage<TimelineItem>> GetChangeHistoryByPersonAsync(Guid personId, ClaimsPrincipal user, PaginationOptions paginationOptions) =>
        WithServiceAsync<ChangeHistoryService, ResultPage<TimelineItem>>(service =>
            service.GetChangeHistoryByPersonAsync(personId, user, paginationOptions));

    private Task<IReadOnlyCollection<ProcessChangeHistoryEntry>> GetChangeHistoryBySupportTaskAsync(string supportTaskReference) =>
        WithServiceAsync<ChangeHistoryService, IReadOnlyCollection<ProcessChangeHistoryEntry>>(service =>
            service.GetChangeHistoryBySupportTaskAsync(supportTaskReference));

    private Task<SupportTask> CreateSupportTaskAsync() =>
        TestData.CreateChangeDateOfBirthRequestSupportTaskAsync();

    private async Task<ClaimsPrincipal> CreatePrincipalAsync(string? role = UserRoles.Administrator)
    {
        var user = await TestData.CreateUserAsync(role: role);
        return new ClaimsPrincipal(new ClaimsIdentity(user.CreateClaims(), authenticationType: "Test"));
    }

    private Task CreateNameChangeEventAsync(Guid personId, string lastName, DateTime createdUtc, Guid? raisedByUserId = null) =>
        WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(new LegacyEvents.PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = createdUtc,
                RaisedBy = raisedByUserId ?? SystemUser.SystemUserId,
                PersonId = personId,
                PersonAttributes = CreatePersonDetails(lastName),
                OldPersonAttributes = CreatePersonDetails("Previous"),
                NameChangeReason = null,
                NameChangeEvidenceFile = null,
                DetailsChangeReason = null,
                DetailsChangeReasonDetail = null,
                DetailsChangeEvidenceFile = null,
                Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.LastName
            });
            await dbContext.SaveChangesAsync();
        });

    private Task<Process> CreateAlertImportingIntoDqtProcessAsync(Guid personId, Guid alertTypeId) =>
        TestData.CreateProcessAsync(
            ProcessType.AlertImportingIntoDqt,
            userId: null,
            changeReason: null,
            new AlertDqtImportedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = personId,
                Alert = new EventModels.Alert
                {
                    AlertId = Guid.NewGuid(),
                    AlertTypeId = alertTypeId,
                    Details = "Test alert details",
                    ExternalLink = null,
                    StartDate = new DateOnly(2024, 1, 1),
                    EndDate = null
                },
                DqtState = 0
            });

    private Task<Process> CreateAlertCreatingProcessAsync(Guid personId, Guid alertTypeId, Guid userId) =>
        TestData.CreateProcessAsync(
            ProcessType.AlertCreating,
            userId,
            changeReason: null,
            new AlertCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = personId,
                Alert = new EventModels.Alert
                {
                    AlertId = Guid.NewGuid(),
                    AlertTypeId = alertTypeId,
                    Details = "Test alert details",
                    ExternalLink = null,
                    StartDate = new DateOnly(2024, 1, 1),
                    EndDate = null
                }
            });

    // Creates the process directly rather than going via TestData so that the DQT user columns can be populated
    private Task<Process> CreateNoteCreatingProcessAsync(
        string supportTaskReference,
        Guid? userId,
        string content = "Some note",
        string? dqtUserName = null) =>
        WithDbContextAsync(async dbContext =>
        {
            IEvent @event = new SupportTaskNoteCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskNote = new EventModels.SupportTaskNote
                {
                    SupportTaskNoteId = Guid.NewGuid(),
                    SupportTaskReference = supportTaskReference,
                    Content = content
                }
            };

            var process = new Process
            {
                ProcessId = Guid.NewGuid(),
                ProcessType = ProcessType.SupportTaskNoteCreating,
                CreatedOn = TimeProvider.UtcNow,
                UpdatedOn = TimeProvider.UtcNow,
                UserId = userId,
                DqtUserId = dqtUserName is not null ? Guid.NewGuid() : null,
                DqtUserName = dqtUserName,
                PersonIds = [],
                OneLoginUserSubjects = [],
                SupportTaskReferences = [.. @event.SupportTaskReferences],
                ChangeReason = null
            };

            dbContext.Processes.Add(process);

            dbContext.Set<ProcessEvent>().Add(new ProcessEvent
            {
                ProcessEventId = @event.EventId,
                ProcessId = process.ProcessId,
                EventName = @event.GetType().Name,
                Payload = @event,
                PersonIds = @event.PersonIds,
                OneLoginUserSubjects = @event.OneLoginUserSubjects,
                SupportTaskReferences = @event.SupportTaskReferences,
                CreatedOn = TimeProvider.UtcNow
            });

            await dbContext.SaveChangesAsync();

            return process;
        });

    private Task<Process> CreateAssigningProcessAsync(Guid userId, params SupportTask[] supportTasks) =>
        TestData.CreateProcessAsync(
            ProcessType.SupportTasksAssigning,
            userId,
            changeReason: null,
            supportTasks
                .Select(t => (IEvent)new SupportTaskUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    SupportTaskReference = t.SupportTaskReference,
                    Changes = SupportTaskUpdatedEventChanges.AssignedToUserId,
                    SupportTask = EventModels.SupportTask.FromModel(t),
                    OldSupportTask = EventModels.SupportTask.FromModel(t),
                    Comments = null,
                    RejectionReason = null
                })
                .ToArray());

    private Task<Process> CreateReactivatingProcessAsync(Guid personId, Guid userId) =>
        TestData.CreateProcessAsync(
            ProcessType.PersonReactivatingInDqt,
            userId,
            changeReason: null,
            new PersonReactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = personId,
                Changes = PersonReactivatedEventChanges.PersonStatus
            });

    private static EventModels.PersonDetails CreatePersonDetails(string lastName) =>
        new()
        {
            FirstName = "Test",
            MiddleName = "User",
            LastName = lastName,
            DateOfBirth = new DateOnly(1990, 1, 1),
            EmailAddress = null,
            NationalInsuranceNumber = null,
            Gender = null
        };

    private static string[] GetNoteContents(IReadOnlyCollection<ProcessChangeHistoryEntry> entries) =>
        entries
            .Select(e => e.GetEvent<SupportTaskNoteCreatedEvent>().SupportTaskNote.Content)
            .ToArray();

    private static string[] GetLastNames(ResultPage<TimelineItem> page) =>
        page
            .Select(i => ((LegacyEventChangeHistoryEntry)i.ItemModel).Event)
            .Cast<LegacyEvents.PersonDetailsUpdatedEvent>()
            .Select(e => e.PersonAttributes.LastName)
            .ToArray();
}
