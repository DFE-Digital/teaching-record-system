using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.Core.Tests.Services.SupportTasks;

[Collection(nameof(DisableParallelization))]
public class SupportTaskServiceTests(SupportTaskService supportTaskService, IClock clock, TestData testData, DbFixture dbFixture)
{
    private IClock Clock => clock;

    private DbFixture DbFixture => dbFixture;

    private TestData TestData => testData;

    [Fact]
    public async Task DeleteSupportTaskAsync_TaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var supportTaskReference = SupportTask.GenerateSupportTaskReference();
        var deleteReason = Faker.Lorem.Paragraph();
        var options = new DeleteSupportTaskOptions(supportTaskReference, deleteReason);

        var processContext = new ProcessContext(ProcessType.SupportTaskDeleting, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await supportTaskService.DeleteSupportTaskAsync(options, processContext);

        // Assert
        Assert.Equal(DeleteSupportTaskResult.NotFound, result);
    }

    [Fact]
    public async Task DeleteSupportTaskAsync_ValidRequest_DeletesSupportTaskAndAddsEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(person.PersonId);
        var deleteReason = Faker.Lorem.Paragraph();
        var options = new DeleteSupportTaskOptions(supportTask.SupportTaskReference, deleteReason);

        var processContext = new ProcessContext(ProcessType.SupportTaskDeleting, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await supportTaskService.DeleteSupportTaskAsync(options, processContext);

        // Assert
        Assert.Equal(DeleteSupportTaskResult.Ok, result);

        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var updatedSupportTask = await dbContext.SupportTasks
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);

            Assert.Equal(Clock.UtcNow, updatedSupportTask?.DeletedOn);
        });

        // TODO Assert event is published when tests are migrated to xUnit 3
    }
}
