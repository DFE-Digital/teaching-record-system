using FakeXrmEasy.Extensions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

public partial class TrsDataSyncHelperTests
{
    [Fact]
    public async Task SyncPersonAsync_NewRecord_WritesNewRowToDb()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var contact = await CreatePersonEntity(contactId);

        // Act
        await Helper.SyncPersonAsync(contact, syncAudit: false, ignoreInvalid: false);

        // Assert
        await AssertDatabasePersonMatchesEntity(contact);
    }

    [Fact]
    public async Task SyncPersonAsync_ExistingRecord_UpdatesExistingRowInDb()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var existingEntity = await CreatePersonEntity(contactId);

        await Helper.SyncPersonAsync(existingEntity, syncAudit: false, ignoreInvalid: false);
        var expectedFirstSync = Clock.UtcNow;

        Clock.Advance();
        var updatedEntity = await CreatePersonEntity(contactId, existingEntity);

        // Act
        await Helper.SyncPersonAsync(updatedEntity, syncAudit: false, ignoreInvalid: false);

        // Assert
        await AssertDatabasePersonMatchesEntity(updatedEntity, expectedFirstSync);
    }

    [Fact]
    public async Task DeleteRecordsAsync_WithPerson_RemovesRowFromDb()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var existingEntity = await CreatePersonEntity(contactId);

        await Helper.SyncPersonAsync(existingEntity, syncAudit: false, ignoreInvalid: false);

        // Act
        await Helper.DeleteRecordsAsync(TrsDataSyncHelper.ModelTypes.Person, new[] { contactId });

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == contactId);
            Assert.Null(person);
        });
    }

    [Fact]
    public async Task SyncPersonAsync_AlreadyHaveNewerVersion_DoesNotUpdateDatabase()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var initialEntity = await CreatePersonEntity(contactId);

        Clock.Advance();
        var updatedEntity = await CreatePersonEntity(contactId, initialEntity);

        await Helper.SyncPersonAsync(updatedEntity, syncAudit: false, ignoreInvalid: false);
        var expectedFirstSync = Clock.UtcNow;
        var expectedLastSync = Clock.UtcNow;

        // Act
        await Helper.SyncPersonAsync(initialEntity, syncAudit: false, ignoreInvalid: false);

        // Assert
        await AssertDatabasePersonMatchesEntity(updatedEntity, expectedFirstSync, expectedLastSync);
    }

    private async Task AssertDatabasePersonMatchesEntity(
        Contact entity,
        DateTime? expectedFirstSync = null,
        DateTime? expectedLastSync = null)
    {
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == entity.Id);
            Assert.NotNull(person);
            Assert.Equal(entity.Id, person.PersonId);
            Assert.Equal(entity.CreatedOn, person.CreatedOn);
            Assert.Equal(entity.ModifiedOn, person.UpdatedOn);
            Assert.Equal(entity.Id, person.DqtContactId);
            Assert.Equal(entity.dfeta_TRN, person.Trn);
            Assert.Equal(entity.FirstName, person.FirstName);
            Assert.Equal(entity.MiddleName, person.MiddleName);
            Assert.Equal(entity.LastName, person.LastName);
            Assert.Equal(entity.BirthDate?.ToDateOnlyWithDqtBstFix(isLocalTime: false), person.DateOfBirth);
            Assert.Equal(entity.EMailAddress1, person.EmailAddress);
            Assert.Equal(entity.dfeta_NINumber, person.NationalInsuranceNumber);
            Assert.Equal(entity.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true), person.QtsDate);
            Assert.Equal(entity.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true), person.EytsDate);
            Assert.Equal((int)entity.StateCode!, person.DqtState);
            Assert.Equal(entity.CreatedOn, person.DqtCreatedOn);
            Assert.Equal(entity.ModifiedOn, person.DqtModifiedOn);
            Assert.Equal(expectedFirstSync ?? Clock.UtcNow, person.DqtFirstSync);
            Assert.Equal(expectedLastSync ?? Clock.UtcNow, person.DqtLastSync);
        });
    }

    private async Task<Contact> CreatePersonEntity(
        Guid contactId,
        Contact? existingContact = null)
    {
        var createdOn = Clock.UtcNow;
        var modifiedOn = Clock.UtcNow;
        var state = ContactState.Active;
        var trn = await TestData.GenerateTrnAsync();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = Faker.Identification.DateOfBirth();
        var email = Faker.Internet.Email();
        var nino = Faker.Identification.UkNationalInsuranceNumber();
        var qtsDate = Clock.Today.AddDays(-40);
        var eytsDate = Clock.Today.AddDays(-30);

        var newContact = existingContact?.Clone<Contact>() ?? new()
        {
            Id = contactId,
            ContactId = contactId,
            CreatedOn = createdOn
        };

        newContact.ModifiedOn = modifiedOn;
        newContact.StateCode = state;
        newContact.dfeta_TRN = trn;
        newContact.FirstName = firstName;
        newContact.MiddleName = middleName;
        newContact.LastName = lastName;
        newContact.BirthDate = dateOfBirth;
        newContact.EMailAddress1 = email;
        newContact.dfeta_NINumber = nino;
        newContact.dfeta_QTSDate = qtsDate.ToDateTimeWithDqtBstFix(isLocalTime: true);
        newContact.dfeta_EYTSDate = eytsDate.ToDateTimeWithDqtBstFix(isLocalTime: true);

        return newContact;
    }

    private Task<EventBase[]> GetEventsForPerson(Guid personId) =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            var results = await dbContext.Database.SqlQuery<EventQueryResult>(
                $"""
                SELECT e.event_name, e.payload
                FROM events as e
                WHERE e.person_id = {personId}
                """).ToArrayAsync();

            return results.Select(r => EventBase.Deserialize(r.Payload, r.EventName)).ToArray();
        });

    private async Task<Contact> CreateUpdatedContactEntityVersion(
        Contact existingContact,
        AuditDetailCollection auditDetailCollection,
        dfeta_InductionStatus? updatedInductionStatus = null,
        DateOnly? updatedQtlsDate = null)
    {
        var currentDqtUser = await TestData.GetCurrentCrmUserAsync();

        var updatedContact = existingContact.Clone<Contact>();
        updatedContact.ModifiedOn = Clock.UtcNow;

        if (updatedQtlsDate is not null)
        {
            updatedContact.dfeta_qtlsdate = updatedQtlsDate.Value.ToDateTimeWithDqtBstFix(isLocalTime: true);

            var oldValueQtlsDate = new Entity(Contact.EntityLogicalName, existingContact.Id);
            oldValueQtlsDate.Attributes[Contact.Fields.ModifiedOn] = existingContact.Attributes[Contact.Fields.ModifiedOn];
            oldValueQtlsDate.Attributes[Contact.Fields.dfeta_qtlsdate] = existingContact.GetAttributeValue<DateTime?>(Contact.Fields.dfeta_qtlsdate);

            var newValueQtlsDate = new Entity(Contact.EntityLogicalName, existingContact.Id);
            newValueQtlsDate.Attributes[Contact.Fields.ModifiedOn] = updatedContact.Attributes[Contact.Fields.ModifiedOn];
            newValueQtlsDate.Attributes[Contact.Fields.dfeta_qtlsdate] = updatedContact.Attributes[Contact.Fields.dfeta_qtlsdate];

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
                OldValue = oldValueQtlsDate,
                NewValue = newValueQtlsDate
            });
        }

        if (updatedInductionStatus is not null)
        {
            updatedContact.dfeta_InductionStatus = updatedInductionStatus;

            var oldValueStatus = new Entity(Contact.EntityLogicalName, existingContact.Id);
            oldValueStatus.Attributes[Contact.Fields.ModifiedOn] = existingContact.Attributes[Contact.Fields.ModifiedOn];
            oldValueStatus.Attributes[Contact.Fields.dfeta_InductionStatus] = existingContact.GetAttributeValue<dfeta_InductionStatus?>(Contact.Fields.dfeta_InductionStatus);

            var newValueStatus = new Entity(Contact.EntityLogicalName, existingContact.Id);
            newValueStatus.Attributes[Contact.Fields.ModifiedOn] = updatedContact.Attributes[Contact.Fields.ModifiedOn];
            newValueStatus.Attributes[Contact.Fields.dfeta_InductionStatus] = updatedContact.Attributes[Contact.Fields.dfeta_InductionStatus];

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
                OldValue = oldValueStatus,
                NewValue = newValueStatus
            });
        }

        return updatedContact;
    }
}
