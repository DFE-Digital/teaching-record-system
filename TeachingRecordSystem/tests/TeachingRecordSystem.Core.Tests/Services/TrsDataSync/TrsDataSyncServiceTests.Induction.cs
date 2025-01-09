using System.Diagnostics;
using FakeXrmEasy.Extensions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

public partial class TrsDataSyncServiceTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Induction_NewRecord_WritesUpdatedPersonRecordToDatabase(bool personAlreadySynced)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithSyncOverride(personAlreadySynced));
        var contactId = person.ContactId;

        var contactAuditDetails = new AuditDetailCollection();
        contactAuditDetails.Add(person.DqtContactAuditDetail);
        await AuditRepository.SetAuditDetailAsync(Contact.EntityLogicalName, contactId, contactAuditDetails);

        var inductionId = Guid.NewGuid();
        var inductionStatus = dfeta_InductionStatus.Pass;
        var inductionStartDate = Clock.Today.AddYears(-1);
        var inductionEndDate = Clock.Today.AddDays(-10);

        var inductionAuditDetails = new AuditDetailCollection();
        var induction = await CreateNewInductionEntityVersion(
            inductionId,
            person.Contact,
            inductionAuditDetails,
            startDate: inductionStartDate,
            endDate: inductionEndDate,
            status: inductionStatus);

        await AuditRepository.SetAuditDetailAsync(dfeta_induction.EntityLogicalName, inductionId, inductionAuditDetails);

        // Keep the contact induction status in sync with dfeta_induction otherwise the sync will fail
        await TestData.OrganizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = contactId,
                dfeta_InductionStatus = dfeta_InductionStatus.Pass
            }
        });

        var newItem = new NewOrUpdatedItem(ChangeType.NewOrUpdated, induction);

        // Act
        await fixture.PublishChangedItemAndConsumeAsync(TrsDataSyncHelper.ModelTypes.Induction, newItem);

        // Assert
        await fixture.DbFixture.WithDbContextAsync(async dbContext =>
        {
            var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == contactId);
            Assert.NotNull(person);
            Assert.Equal(inductionStatus.ToInductionStatus(), person.InductionStatus);
            Assert.Equal(inductionStartDate, person.InductionStartDate);
            Assert.Equal(inductionEndDate, person.InductionCompletedDate);
        });
    }

    [Fact]
    public async Task Induction_UpdatedRecord_WritesUpdatedPersonRecordToDatabase()
    {
        // Arrange
        var originalInductionStatus = dfeta_InductionStatus.InProgress;
        var originalInductionStartDate = Clock.Today.AddYears(-1);
        var originalInductionEndDate = Clock.Today.AddDays(-10);

        var person = await TestData.CreatePersonAsync(
            p => p.WithSyncOverride(true)
                .WithDqtInduction(originalInductionStatus, null, originalInductionStartDate, null));
        var contactId = person.ContactId;

        var contactAuditDetails = new AuditDetailCollection();
        contactAuditDetails.Add(person.DqtContactAuditDetail);
        await AuditRepository.SetAuditDetailAsync(Contact.EntityLogicalName, contactId, contactAuditDetails);

        var inductionId = person.DqtInductions.Single().InductionId;
        using var ctx = new DqtCrmServiceContext(TestData.OrganizationService);
        var induction = ctx.dfeta_inductionSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_induction.PrimaryIdAttribute) == inductionId);
        var inductionAuditDetails = new AuditDetailCollection();

        var updatedInduction = await CreateUpdatedInductionEntityVersion(
            induction!,
            inductionAuditDetails,
            DqtInductionUpdatedEventChanges.StartDate & DqtInductionUpdatedEventChanges.CompletionDate | DqtInductionUpdatedEventChanges.Status);

        await AuditRepository.SetAuditDetailAsync(dfeta_induction.EntityLogicalName, inductionId, inductionAuditDetails);

        // Keep the contact induction status in sync with dfeta_induction otherwise the sync will fail
        await TestData.OrganizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = contactId,
                dfeta_InductionStatus = updatedInduction.dfeta_InductionStatus
            }
        });

        var updatedItem = new NewOrUpdatedItem(ChangeType.NewOrUpdated, updatedInduction);

        // Act
        await fixture.PublishChangedItemAndConsumeAsync(TrsDataSyncHelper.ModelTypes.Induction, updatedItem);

        // Assert
        await fixture.DbFixture.WithDbContextAsync(async dbContext =>
        {
            var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == contactId);
            Assert.NotNull(person);
            Assert.Equal(updatedInduction.dfeta_InductionStatus.ToInductionStatus(), person.InductionStatus);
            Assert.Equal(updatedInduction.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true), person.InductionStartDate);
            Assert.Equal(updatedInduction.dfeta_CompletionDate.ToDateOnlyWithDqtBstFix(isLocalTime: true), person.InductionCompletedDate);
            Assert.Equal(updatedInduction.ModifiedOn, person.DqtInductionModifiedOn);
        });
    }

    private async Task<dfeta_induction> CreateNewInductionEntityVersion(
        Guid inductionId,
        Contact contact,
        AuditDetailCollection auditDetailCollection,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        dfeta_InductionStatus? status = null,
        dfeta_InductionExemptionReason? exemptionReason = null,
        bool addCreateAudit = true)
    {
        Debug.Assert(auditDetailCollection.Count == 0);

        var currentDqtUser = await TestData.GetCurrentCrmUserAsync();
        var createdOn = Clock.UtcNow;
        var modifiedOn = Clock.UtcNow;
        var state = dfeta_inductionState.Active;

        var newInduction = new dfeta_induction()
        {
            Id = inductionId,
            dfeta_PersonId = contact.Id.ToEntityReference(Contact.EntityLogicalName),
            CreatedOn = createdOn,
            CreatedBy = currentDqtUser,
            ModifiedOn = modifiedOn,
            StateCode = state,
            dfeta_StartDate = startDate?.ToDateTimeWithDqtBstFix(isLocalTime: true),
            dfeta_CompletionDate = endDate?.ToDateTimeWithDqtBstFix(isLocalTime: true),
            dfeta_InductionStatus = status,
            dfeta_InductionExemptionReason = exemptionReason
        };

        if (addCreateAudit)
        {
            var auditId = Guid.NewGuid();
            auditDetailCollection.Add(new AttributeAuditDetail()
            {
                AuditRecord = new Audit()
                {
                    Action = Audit_Action.Create,
                    AuditId = auditId,
                    CreatedOn = Clock.UtcNow,
                    Id = auditId,
                    Operation = Audit_Operation.Create,
                    UserId = currentDqtUser
                },
                OldValue = new Entity(dfeta_induction.EntityLogicalName),
                NewValue = newInduction.Clone()
            });
        }

        return newInduction;
    }

    private async Task<dfeta_induction> CreateUpdatedInductionEntityVersion(
        dfeta_induction existingInduction,
        AuditDetailCollection auditDetailCollection,
        DqtInductionUpdatedEventChanges? changes = null)
    {
        if (changes == DqtInductionUpdatedEventChanges.None)
        {
            throw new ArgumentException("Changes cannot be None.", nameof(changes));
        }

        bool ChangeRequested(DqtInductionUpdatedEventChanges field) =>
            changes is null || changes.Value.HasFlag(field);

        var currentDqtUser = await TestData.GetCurrentCrmUserAsync();

        var existingStartDate = existingInduction.dfeta_StartDate;
        var startDate = ChangeRequested(DqtInductionUpdatedEventChanges.StartDate) ?
            (existingInduction.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true) is DateOnly existingStartDateOnly ?
                TestData.GenerateChangedDate(existingStartDateOnly, min: new DateOnly(2020, 4, 1)) :
                TestData.GenerateDate(min: new DateOnly(2020, 4, 1))).ToDateTimeWithDqtBstFix(isLocalTime: true) :
            existingStartDate;

        var existingCompletionDate = existingInduction.dfeta_CompletionDate;
        DateTime? completionDate;

        if (ChangeRequested(DqtInductionUpdatedEventChanges.CompletionDate))
        {
            if (startDate is null)
            {
                throw new InvalidOperationException("Cannot generate a completion date when there is no start date.");
            }

            var startDateOnly = startDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true);

            completionDate = (existingCompletionDate is null ?
                    TestData.GenerateDate(min: startDateOnly.AddDays(1)) :
                    TestData.GenerateChangedDate(existingCompletionDate.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true), min: startDateOnly.AddDays(1)))
                .ToDateTimeWithDqtBstFix(isLocalTime: true);
        }
        else
        {
            completionDate = null;
        }

        var existingStatus = existingInduction.dfeta_InductionStatus;
        var status = ChangeRequested(DqtInductionUpdatedEventChanges.Status) ?
            TestData.GenerateChangedEnumValue(existingStatus) :
            existingStatus;

        var existingExemptionReason = existingInduction.dfeta_InductionExemptionReason;
        var exemptionReason = ChangeRequested(DqtInductionUpdatedEventChanges.ExemptionReason) ?
            TestData.GenerateChangedEnumValue(existingExemptionReason) :
            existingExemptionReason;

        var updatedInduction = existingInduction.Clone<dfeta_induction>();
        updatedInduction.ModifiedOn = Clock.UtcNow;
        updatedInduction.dfeta_StartDate = startDate;
        updatedInduction.dfeta_CompletionDate = completionDate;
        updatedInduction.dfeta_InductionStatus = status;
        updatedInduction.dfeta_InductionExemptionReason = exemptionReason;

        var changedAttrs = (
            from newAttr in updatedInduction.Attributes
            join oldAttr in existingInduction.Attributes on newAttr.Key equals oldAttr.Key
            where !AttributeValuesEqual(newAttr.Value, oldAttr.Value)
            select newAttr.Key).ToArray();

        var oldValue = new Entity(dfeta_induction.EntityLogicalName, existingInduction.Id);
        Array.ForEach(changedAttrs, a => oldValue.Attributes[a] = existingInduction.Attributes[a]);

        var newValue = new Entity(dfeta_induction.EntityLogicalName, existingInduction.Id);
        Array.ForEach(changedAttrs, a => newValue.Attributes[a] = updatedInduction.Attributes[a]);


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

        return updatedInduction;

        static bool AttributeValuesEqual(object? a, object? b) =>
            a is null && b is null ||
            (a is not null && b is not null && a.Equals(b));
    }
}
