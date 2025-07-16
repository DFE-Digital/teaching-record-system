using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.Files;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class InductionStatusUpdatedSupportJobTests : IAsyncLifetime
{
    public InductionStatusUpdatedSupportJobTests(
        DbFixture dbFixture,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator,
        IServiceProvider provider)
    {
        OrganizationService = provider.GetService<IOrganizationServiceAsync2>()!;
        DbFixture = dbFixture;
        Clock = new();

        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            OrganizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataPersonDataSource.CrmAndTrs);

        Provider = provider;
        Job = ActivatorUtilities.CreateInstance<InductionStatusUpdatedSupportJob>(provider, Clock);
    }

    public DbFixture DbFixture { get; }

    public TestData TestData { get; }

    public TestableClock Clock { get; }

    public IServiceProvider Provider { get; }

    public InductionStatusUpdatedSupportJob Job { get; }

    public async Task InitializeAsync()
    {
        await DbFixture.DbHelper.ClearDataAsync();
        await using var context = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        context.JobMetadata.Add(new JobMetadata()
        {
            JobName = nameof(InductionStatusUpdatedSupportJob),
            Metadata = new Dictionary<string, object>
            {
                { "LastRunDate", Clock.UtcNow.AddDays(-1) }
            }
        });
        context.SaveChanges();
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    public IOrganizationServiceAsync2 OrganizationService { get; }

    public Mock<IFileService> BlobStorageFileService { get; } = new Mock<IFileService>();

    [Theory]
    [InlineData(InductionStatus.Exempt, InductionStatus.None)]
    [InlineData(InductionStatus.Exempt, InductionStatus.RequiredToComplete)]
    public async Task Execute_WhenCalled_FromExemptToNoneOrRequiredToComplete_CreatesSupportTask(InductionStatus fromInductionStatus, InductionStatus toInductionStatus)
    {
        // Arrange
        await using var context = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var jobMetaData = await context.JobMetadata.SingleAsync(i => i.JobName == nameof(InductionStatusUpdatedSupportJob));
        jobMetaData.Metadata = new Dictionary<string, object>
        {
            {"LastRunDate", Clock.UtcNow.AddDays(-1) }
        };
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trsPerson = context.Persons.Single(x => x.Trn == person.Trn);
        trsPerson.SetInductionStatus(
            fromInductionStatus,
            startDate: null,
            completedDate: null,
            exemptionReasonIds: new[] { InductionExemptionReason.PassedInWalesId },
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out var event1);
        context.AddEventWithoutBroadcast(event1!);
        trsPerson.SetInductionStatus(
            toInductionStatus,
            startDate: null,
            completedDate: null,
            exemptionReasonIds: Array.Empty<Guid>(),
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow.AddDays(1),
            out var event2);
        context.AddEventWithoutBroadcast(event2!);
        await context.SaveChangesAsync();

        // Act
        await Job.ExecuteAsync(CancellationToken.None);

        // Assert
        var task = ctx.TaskSet.SingleOrDefault(x => x.RegardingObjectId == person.ContactId.ToEntityReference(Contact.EntityLogicalName));
        Assert.NotNull(task);
        Assert.Equal($"Induction Status Updated from {fromInductionStatus.ToString()} to {toInductionStatus.ToString()}", task.Description);
        Assert.Equal("Induction Status Updated", task.Category);
        Assert.Equal("Induction Status Updated", task.Subject);
    }

    [Fact]
    public async Task Execute_WhenCalled_FromInProgressToExempt_CreatesSupportTask()
    {
        // Arrange
        var fromInductionStatus = Models.InductionStatus.InProgress;
        var toInductionStatus = InductionStatus.Exempt;
        await using var context = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var jobMetaData = await context.JobMetadata.SingleAsync(i => i.JobName == nameof(InductionStatusUpdatedSupportJob));
        jobMetaData.Metadata = new Dictionary<string, object>
        {
            { "LastRunDate", Clock.UtcNow.AddDays(-1) }
        };
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trsPerson = context.Persons.Single(x => x.Trn == person.Trn);
        trsPerson.SetInductionStatus(
            fromInductionStatus,
            startDate: null,
            completedDate: null,
            exemptionReasonIds: Array.Empty<Guid>(),
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow,
            out var event1);
        context.AddEventWithoutBroadcast(event1!);

        trsPerson.SetInductionStatus(
            toInductionStatus,
            startDate: null,
            completedDate: null,
            exemptionReasonIds: new[] { InductionExemptionReason.PassedInWalesId },
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow.AddDays(1),
            out var event2);
        context.AddEventWithoutBroadcast(event2!);
        await context.SaveChangesAsync();

        // Act
        await Job.ExecuteAsync(CancellationToken.None);

        // Assert
        var task = ctx.TaskSet.SingleOrDefault(x => x.RegardingObjectId == person.ContactId.ToEntityReference(Contact.EntityLogicalName));
        Assert.NotNull(task);
        Assert.Equal($"Induction Status Updated from {fromInductionStatus.ToString()} to {toInductionStatus.ToString()}", task.Description);
        Assert.Equal("Induction Status Updated", task.Category);
        Assert.Equal("Induction Status Updated", task.Subject);
    }

    [Fact]
    public async Task Execute_LastRunDateAfterInductionUpdates_NoTasksCreated()
    {
        // Arrange
        var fromInductionStatus = Models.InductionStatus.InProgress;
        var toInductionStatus = InductionStatus.Exempt;
        await using var context = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var jobMetaData = await context.JobMetadata.SingleAsync(i => i.JobName == nameof(InductionStatusUpdatedSupportJob));
        jobMetaData.Metadata = new Dictionary<string, object>
        {
            { "LastRunDate", Clock.UtcNow.AddDays(-1) }
        };
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var trsPerson = context.Persons.Single(x => x.Trn == person.Trn);
        trsPerson.SetInductionStatus(
            fromInductionStatus,
            startDate: null,
            completedDate: null,
            exemptionReasonIds: Array.Empty<Guid>(),
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow.AddDays(-2),
            out var event1);
        context.AddEventWithoutBroadcast(event1!);

        trsPerson.SetInductionStatus(
            toInductionStatus,
            startDate: null,
            completedDate: null,
            exemptionReasonIds: new[] { InductionExemptionReason.PassedInWalesId },
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            updatedBy: SystemUser.SystemUserId,
            Clock.UtcNow.AddDays(-2),
            out var event2);
        context.AddEventWithoutBroadcast(event2!);
        await context.SaveChangesAsync();

        // Act
        await Job.ExecuteAsync(CancellationToken.None);

        // Assert
        var task = ctx.TaskSet.FirstOrDefault(x => x.RegardingObjectId == person.ContactId.ToEntityReference(Contact.EntityLogicalName));
        Assert.Null(task);
    }

    [Fact]
    public async Task Execute_MetadataDoesNotExist_InsertsRow()
    {
        // Arrange
        await using var context = await DbFixture.DbHelper.DbContextFactory.CreateDbContextAsync();
        using var ctx = new DqtCrmServiceContext(OrganizationService);
        var options = Options.Create(new InductionStatusUpdatedSupportJobOptions()
        {
            InitialLastRunDate = Clock.UtcNow,
        });
        var crmDispatcher = Provider.GetService<ICrmQueryDispatcher>();
        var inductionStatusJob = new InductionStatusUpdatedSupportJob(context, crmDispatcher!, Clock, options);

        // Act
        await inductionStatusJob.ExecuteAsync(CancellationToken.None);

        // Assert
        var insertedJobMetaData = await context.JobMetadata.SingleAsync(i => i.JobName == nameof(InductionStatusUpdatedSupportJob));
        Assert.NotNull(insertedJobMetaData);
    }
}
