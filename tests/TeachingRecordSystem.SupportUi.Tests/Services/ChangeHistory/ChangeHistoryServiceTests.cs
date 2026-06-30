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
    public async Task GetChangeHistoryByPersonAsync_FiltersAlertEventsByAlertTypeReadPermission(bool isDbsAlertType, string? role, bool shouldInclude)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var alertType = isDbsAlertType
            ? await TestData.ReferenceDataCache.GetAlertTypeByIdAsync(AlertType.DbsAlertTypeId)
            : (await TestData.ReferenceDataCache.GetAlertTypesAsync()).First(t => !t.IsDbsAlertType);

        await CreateAlertImportedEventAsync(person.PersonId, alertType.AlertTypeId);

        var principal = await CreatePrincipalAsync(role);

        // Act
        var result = await GetChangeHistoryByPersonAsync(person.PersonId, principal, new());

        // Assert
        if (shouldInclude)
        {
            var item = Assert.Single(result);
            Assert.Equal(TimelineItemType.LegacyEvent, item.ItemType);
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

    private Task<ResultPage<TimelineItem>> GetChangeHistoryByPersonAsync(Guid personId, ClaimsPrincipal user, PaginationOptions paginationOptions) =>
        WithServiceAsync<ChangeHistoryService, ResultPage<TimelineItem>>(service =>
            service.GetChangeHistoryByPersonAsync(personId, user, paginationOptions));

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

    private Task CreateAlertImportedEventAsync(Guid personId, Guid alertTypeId) =>
        WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(new LegacyEvents.AlertDqtImportedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = TimeProvider.UtcNow,
                RaisedBy = SystemUser.SystemUserId,
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
            await dbContext.SaveChangesAsync();
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

    private static string[] GetLastNames(ResultPage<TimelineItem> page) =>
        page
            .Select(i => ((LegacyEventChangeHistoryEntry)i.ItemModel).Event)
            .Cast<LegacyEvents.PersonDetailsUpdatedEvent>()
            .Select(e => e.PersonAttributes.LastName)
            .ToArray();
}
