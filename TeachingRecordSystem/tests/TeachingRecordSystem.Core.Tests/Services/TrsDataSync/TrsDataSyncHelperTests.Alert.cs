using System.Diagnostics;
using Microsoft.Crm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

public partial class TrsDataSyncHelperTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SyncAlert_NewRecord_WritesNewRowToDb(bool personAlreadySynced)
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithSyncOverride(personAlreadySynced));
        var alertId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var entity = await CreateNewAlertEntityVersion(alertId, person.ContactId, auditDetailCollection);

        // Act
        await Helper.SyncAlert(entity, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        await AssertDatabaseAlertMatchesEntity(entity);
    }

    [Fact]
    public async Task SyncAlert_SanctionCodeIsRedundant_IsNotWrittenToDb()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var entity = await CreateNewAlertEntityVersion(alertId, person.ContactId, auditDetailCollection, redundantType: true);

        // Act
        await Helper.SyncAlert(entity, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        await DbFixture.WithDbContext(async dbContext =>
        {
            var alert = await dbContext.Alerts.SingleOrDefaultAsync(p => p.DqtSanctionId == entity.Id);
            Assert.Null(alert);
        });
    }

    private async Task<dfeta_sanction> CreateNewAlertEntityVersion(
        Guid sanctionId,
        Guid personContactId,
        AuditDetailCollection auditDetailCollection,
        bool redundantType = false)
    {
        Debug.Assert(auditDetailCollection.Count == 0);

        var sanctionCodes = await TestData.ReferenceDataCache.GetSanctionCodes(activeOnly: false);
        var alertTypes = await TestData.ReferenceDataCache.GetAlertTypes();
        var currentDqtUser = await TestData.GetCurrentCrmUser();

        var createdOn = Clock.UtcNow;
        var modifiedOn = Clock.UtcNow;
        var state = dfeta_sanctionState.Active;

        // Redundant sanction types won't have a corresponding AlertType (and shouldn't be migrated)
        var sanctionCodeId = sanctionCodes
            .Where(sc => (!redundantType && alertTypes.Any(t => t.DqtSanctionCode == sc.dfeta_Value)) ||
                (redundantType && !alertTypes.Any(t => t.DqtSanctionCode == sc.dfeta_Value)))
            .RandomOne().Id;

        var details = Faker.Lorem.Paragraph();
        var externalLink = Faker.Internet.Url();
        var startDate = new DateOnly(2020, 4, Random.Shared.Next(1, 30)).ToDateTimeWithDqtBstFix(isLocalTime: true);
        var endDate = new[] { true, false }.RandomOne() ? (DateTime?)startDate.AddMonths(6) : null;
        var spent = endDate.HasValue;

        var newSanction = new dfeta_sanction()
        {
            Id = sanctionId,
            dfeta_sanctionId = sanctionId,
            dfeta_PersonId = personContactId.ToEntityReference(Contact.EntityLogicalName),
            CreatedOn = createdOn,
            CreatedBy = currentDqtUser,
            ModifiedOn = modifiedOn,
            StateCode = state,
            dfeta_DetailsLink = externalLink,
            dfeta_SanctionDetails = details,
            dfeta_StartDate = startDate,
            dfeta_EndDate = endDate,
            dfeta_Spent = spent,
            dfeta_SanctionCodeId = sanctionCodeId.ToEntityReference(dfeta_sanctioncode.EntityLogicalName),
        };

        return newSanction;
    }

    private async Task AssertDatabaseAlertMatchesEntity(dfeta_sanction entity)
    {
        await DbFixture.WithDbContext(async dbContext =>
        {
            var sanctionCode = await TestData.ReferenceDataCache.GetSanctionCodeById(entity.dfeta_SanctionCodeId.Id);
            var alertTypes = await TestData.ReferenceDataCache.GetAlertTypes();
            var expectedAlertType = alertTypes.Single(t => t.DqtSanctionCode == sanctionCode.dfeta_Value);

            var alert = await dbContext.Alerts.SingleOrDefaultAsync(p => p.DqtSanctionId == entity.Id);
            Assert.NotNull(alert);
            Assert.Equal(entity.Id, alert.AlertId);
            Assert.Equal(entity.CreatedOn, alert.CreatedOn);
            Assert.Equal(entity.ModifiedOn, alert.UpdatedOn);
            Assert.Null(alert.DeletedOn);
            Assert.Equal(entity.dfeta_PersonId?.Id, alert.PersonId);
            Assert.Equal((int)entity.StateCode!, alert.DqtState);
            Assert.Equal(entity.CreatedOn, alert.DqtCreatedOn);
            Assert.Equal(entity.ModifiedOn, alert.DqtModifiedOn);
            Assert.Equal(expectedAlertType.AlertTypeId, alert.AlertTypeId);
            Assert.Equal(entity.dfeta_SanctionDetails, alert.Details);
            Assert.Equal(entity.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), alert.StartDate);
            Assert.Equal(entity.dfeta_EndDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), alert.EndDate);
            Assert.Equal(entity.dfeta_DetailsLink, alert.ExternalLink);
        });
    }
}
