using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.OneLogins.OneLoginDetail;

public class EventsTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithMultipleProcesses_ShowsMostRecentProcessFirst()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateOneLoginUserAsync(person);
        var oneLoginUser = EventModels.OneLoginUser.FromModel(user);

        ProcessType[] processTypes =
        [
            ProcessType.TeacherSigningIn,
            ProcessType.PersonOneLoginUserConnecting,
            ProcessType.PersonOneLoginUserDisconnecting,
            ProcessType.OneLoginUserPersonConnecting
        ];

        foreach (var processType in processTypes)
        {
            await TestData.CreateProcessAsync(
                processType,
                events:
                [
                    new OneLoginUserSignedInEvent
                    {
                        EventId = Guid.NewGuid(),
                        OneLoginUser = oneLoginUser
                    }
                ]);

            TimeProvider.Advance(TimeSpan.FromHours(1));
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}/events");

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
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateOneLoginUserAsync(person);
        var oneLoginUser = EventModels.OneLoginUser.FromModel(user);

        var updatedEventId = ProcessTimelineTestHelper.HighSortingId();

        await TestData.CreateProcessAsync(
            ProcessType.TeacherSigningIn,
            events:
            [
                new OneLoginUserSignedInEvent
                {
                    EventId = ProcessTimelineTestHelper.LowSortingId(),
                    OneLoginUser = oneLoginUser
                },
                new OneLoginUserUpdatedEvent
                {
                    EventId = updatedEventId,
                    OneLoginUser = oneLoginUser,
                    OldOneLoginUser = oneLoginUser with { PersonId = null },
                    Changes = OneLoginUserUpdatedEventChanges.PersonId
                }
            ]);

        // The clock is frozen, so every event in the process gets the same timestamp; pull the update
        // ahead of the sign in, the opposite of the order they were added in
        await SetProcessEventCreatedOnAsync(updatedEventId, TimeProvider.UtcNow.AddMinutes(-1));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}/events");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal(
            [nameof(OneLoginUserUpdatedEvent), nameof(OneLoginUserSignedInEvent)],
            ProcessTimelineTestHelper.GetEventNames(doc));
    }

    private Task SetProcessEventCreatedOnAsync(Guid processEventId, DateTime createdOn) =>
        WithDbContextAsync(dbContext => dbContext.Set<ProcessEvent>()
            .Where(e => e.ProcessEventId == processEventId)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.CreatedOn, createdOn)));
}
