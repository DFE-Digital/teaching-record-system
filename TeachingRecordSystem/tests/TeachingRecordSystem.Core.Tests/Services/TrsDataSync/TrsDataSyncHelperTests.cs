using FakeXrmEasy.Extensions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

[Collection(nameof(TrsDataSyncTestCollection))]
public partial class TrsDataSyncHelperTests : IAsyncLifetime
{
    public TrsDataSyncHelperTests(
        DbFixture dbFixture,
        IOrganizationServiceAsync2 organizationService,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
    {
        DbFixture = dbFixture;
        Clock = new();

        Helper = new TrsDataSyncHelper(
            dbFixture.GetDataSource(),
            organizationService,
            referenceDataCache,
            Clock,
            new TestableAuditRepository(),
            loggerFactory.CreateLogger<TrsDataSyncHelper>(),
            BlobStorageFileService.Object,
            configuration);

        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            organizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataPersonDataSource.CrmAndTrs);
    }

    private DbFixture DbFixture { get; }

    private TestData TestData { get; }

    private TestableClock Clock { get; }

    public TrsDataSyncHelper Helper { get; }

    public Mock<IFileService> BlobStorageFileService { get; } = new Mock<IFileService>();

    Task IAsyncLifetime.InitializeAsync() => DbFixture.WithDbContextAsync(dbContext => dbContext.Events.ExecuteDeleteAsync());

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    private async Task<T> CreateDeactivatedEntityVersionAsync<T>(
        T existingEntity,
        string entityLogicalName,
        AuditDetailCollection auditDetailCollection)
        where T : Entity
    {
        if (existingEntity.GetAttributeValue<OptionSetValue>("statecode").Value != 0)
        {
            throw new ArgumentException("Entity must be active.", nameof(existingEntity));
        }

        var currentDqtUser = await TestData.GetCurrentCrmUserAsync();

        var updatedEntity = existingEntity.Clone();
        updatedEntity.Attributes["statecode"] = new OptionSetValue(1);
        updatedEntity.Attributes["statuscode"] = new OptionSetValue(2);

        var oldValue = new Entity(entityLogicalName, existingEntity.Id);
        oldValue.Attributes["statecode"] = new OptionSetValue(0);
        oldValue.Attributes["statuscode"] = new OptionSetValue(1);

        var newValue = new Entity(entityLogicalName, existingEntity.Id);
        newValue.Attributes["statecode"] = new OptionSetValue(1);
        newValue.Attributes["statuscode"] = new OptionSetValue(2);

        var auditId = Guid.NewGuid();
        auditDetailCollection.Add(new AttributeAuditDetail()
        {
            AuditRecord = new Audit()
            {
                Action = Audit_Action.Update,
                AuditId = auditId,
                CreatedOn = Clock.UtcNow,
                Id = auditId,
                Operation = Audit_Operation.Update,
                UserId = currentDqtUser
            },
            OldValue = oldValue,
            NewValue = newValue
        });

        return updatedEntity.ToEntity<T>();
    }

    private async Task<T> CreateReactivatedEntityVersionAsync<T>(
        T existingEntity,
        string entityLogicalName,
        AuditDetailCollection auditDetailCollection)
        where T : Entity
    {
        if (existingEntity.GetAttributeValue<OptionSetValue>("statecode").Value != 1)
        {
            throw new ArgumentException("Entity must be inactive.", nameof(existingEntity));
        }

        var currentDqtUser = await TestData.GetCurrentCrmUserAsync();

        var updatedEntity = existingEntity.Clone();
        updatedEntity.Attributes["statecode"] = new OptionSetValue(0);
        updatedEntity.Attributes["statuscode"] = new OptionSetValue(1);

        var oldValue = new Entity(entityLogicalName, existingEntity.Id);
        oldValue.Attributes["statecode"] = new OptionSetValue(1);
        oldValue.Attributes["statuscode"] = new OptionSetValue(2);

        var newValue = new Entity(entityLogicalName, existingEntity.Id);
        newValue.Attributes["statecode"] = new OptionSetValue(0);
        newValue.Attributes["statuscode"] = new OptionSetValue(1);

        var auditId = Guid.NewGuid();
        auditDetailCollection.Add(new AttributeAuditDetail()
        {
            AuditRecord = new Audit()
            {
                Action = Audit_Action.Update,
                AuditId = auditId,
                CreatedOn = Clock.UtcNow,
                Id = auditId,
                Operation = Audit_Operation.Update,
                UserId = currentDqtUser
            },
            OldValue = oldValue,
            NewValue = newValue
        });

        return updatedEntity.ToEntity<T>();
    }

    private class EventQueryResult
    {
        public required string EventName { get; set; }
        public required string Payload { get; set; }
    }
}
