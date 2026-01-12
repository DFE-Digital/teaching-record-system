using Optional.Unsafe;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Tests.EventPipelineTests;

public class SupportTaskEventPipelineTests(EventPipelineFixture fixture) : EventPipelineTestBase(fixture)
{
    [Fact]
    public async Task SupportTaskCreatedEventPublished_EmitsLegacySupportTaskCreatedEvent()
    {
        // Arrange
        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var @event = new SupportTaskCreatedEvent
        {
            EventId = Guid.NewGuid(),
            SupportTask = new EventModels.SupportTask
            {
                SupportTaskReference = "ABC-123",
                SupportTaskType = SupportTaskType.ChangeDateOfBirthRequest,
                Status = SupportTaskStatus.Open,
                OneLoginUserSubject = null,
                PersonId = Guid.NewGuid(),
                Data = new ChangeDateOfBirthRequestData
                {
                    DateOfBirth = new(2000, 1, 1),
                    EvidenceFileId = Guid.NewGuid(),
                    EvidenceFileName = "evidence.jpeg",
                    EmailAddress = TestData.GenerateUniqueEmail(),
                    ChangeRequestOutcome = null
                },
                ResolveJourneySavedState = null
            }
        };

        // Act
        await EventPublisher.PublishEventAsync(@event, processContext);

        // Assert
        LegacyEventObserver.AssertEventsSaved(
            e =>
            {
                var legacyEvent = Assert.IsType<LegacyEvents.SupportTaskCreatedEvent>(e);
                Assert.Equal(@event.EventId, legacyEvent.EventId);
                Assert.Equal(@event.SupportTask, legacyEvent.SupportTask);
                Assert.Equal(processContext.Now, legacyEvent.CreatedUtc);
                Assert.Equal(@event.PersonIds.SingleOrDefault(), legacyEvent.PersonId.ToNullable());
            });
    }
}
