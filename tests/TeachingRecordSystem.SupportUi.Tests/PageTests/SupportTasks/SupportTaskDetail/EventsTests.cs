using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.SupportTaskDetail;

public class EventsTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithMultipleProcesses_ShowsMostRecentProcessFirst()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);
        var supportTaskEventModel = EventModels.SupportTask.FromModel(supportTask);

        ProcessType[] processTypes =
        [
            ProcessType.TeacherSigningIn,
            ProcessType.SupportTaskNoteCreating,
            ProcessType.SupportTaskAllocating,
            ProcessType.SupportTaskZendeskUrlsUpdating
        ];

        foreach (var processType in processTypes)
        {
            await TestData.CreateProcessAsync(
                processType,
                events: [CreateSupportTaskUpdatedEvent(supportTaskEventModel, Guid.NewGuid())]);

            TimeProvider.Advance(TimeSpan.FromHours(1));
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/{supportTask.SupportTaskReference}/events");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal(
            processTypes.Reverse().Select(t => t.ToString()).ToArray(),
            ProcessTimelineTestHelper.GetProcessTypes(doc));
    }

    [Fact]
    public async Task Get_WithMultipleEventsInProcess_ShowsOldestEventFirst()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);
        var supportTaskEventModel = EventModels.SupportTask.FromModel(supportTask);

        var updatedEventId = ProcessTimelineTestHelper.HighSortingId();

        await TestData.CreateProcessAsync(
            ProcessType.TeacherSigningIn,
            events:
            [
                CreateSupportTaskCreatedEvent(supportTaskEventModel, ProcessTimelineTestHelper.LowSortingId()),
                CreateSupportTaskUpdatedEvent(supportTaskEventModel, updatedEventId)
            ]);

        // The clock is frozen, so every event in the process gets the same timestamp; pull the update
        // ahead of the created event, the opposite of the order they were added in
        await SetProcessEventCreatedOnAsync(updatedEventId, TimeProvider.UtcNow.AddMinutes(-1));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/{supportTask.SupportTaskReference}/events");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal(
            [nameof(SupportTaskUpdatedEvent), nameof(SupportTaskCreatedEvent)],
            ProcessTimelineTestHelper.GetEventNames(doc));
    }

    private static SupportTaskCreatedEvent CreateSupportTaskCreatedEvent(EventModels.SupportTask supportTask, Guid eventId) =>
        new()
        {
            EventId = eventId,
            SupportTask = supportTask
        };

    private static SupportTaskUpdatedEvent CreateSupportTaskUpdatedEvent(EventModels.SupportTask supportTask, Guid eventId) =>
        new()
        {
            EventId = eventId,
            SupportTaskReference = supportTask.SupportTaskReference,
            Changes = SupportTaskUpdatedEventChanges.Status,
            SupportTask = supportTask,
            OldSupportTask = supportTask with { Status = SupportTaskStatus.InProgress },
            Comments = null,
            RejectionReason = null
        };

    private Task SetProcessEventCreatedOnAsync(Guid processEventId, DateTime createdOn) =>
        WithDbContextAsync(dbContext => dbContext.Set<ProcessEvent>()
            .Where(e => e.ProcessEventId == processEventId)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.CreatedOn, createdOn)));
}
