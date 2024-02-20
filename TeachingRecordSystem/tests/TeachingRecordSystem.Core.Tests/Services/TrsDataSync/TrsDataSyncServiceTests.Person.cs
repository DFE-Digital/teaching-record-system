using FakeXrmEasy.Extensions;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

public partial class TrsDataSyncServiceTests
{
    [Fact]
    public async Task Contact_NewRecord_WritesNewPersonRecordToDatabase()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePerson(b => b.WithSyncOverride(false));
        var contactId = createPersonResult.ContactId;
        var contact = createPersonResult.Contact;
        contact.CreatedOn = Clock.UtcNow;
        contact.ModifiedOn = Clock.UtcNow;
        var newItem = new NewOrUpdatedItem(ChangeType.NewOrUpdated, contact);

        // Act
        await fixture.PublishChangedItemAndConsume(TrsDataSyncHelper.ModelTypes.Person, newItem);

        // Assert
        await fixture.DbFixture.WithDbContext(async dbContext =>
        {
            var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == contactId);
            Assert.NotNull(person);
            Assert.Equal(fixture.Clock.UtcNow, person.DqtFirstSync);
            Assert.Equal(fixture.Clock.UtcNow, person.DqtLastSync);
        });
    }

    [Fact]
    public async Task Contact_UpdatedRecord_WritesUpdatedPersonRecordToDatabase()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePerson(b => b.WithSyncOverride(false));
        var contactId = createPersonResult.ContactId;
        var contact = createPersonResult.Contact;
        contact.CreatedOn = Clock.UtcNow;
        contact.ModifiedOn = Clock.UtcNow;

        await fixture.Helper.SyncPerson(contact, ignoreInvalid: false);
        var expectedFirstSync = Clock.UtcNow;

        var modifiedOn = Clock.Advance();

        var updatedContact = createPersonResult.Contact.Clone<Contact>();
        updatedContact.StateCode = ContactState.Inactive;
        updatedContact.ModifiedOn = fixture.Clock.UtcNow;

        var updatedItem = new NewOrUpdatedItem(ChangeType.NewOrUpdated, updatedContact);

        // Act
        await fixture.PublishChangedItemAndConsume(TrsDataSyncHelper.ModelTypes.Person, updatedItem);

        // Assert
        await fixture.DbFixture.WithDbContext(async dbContext =>
        {
            var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == contactId);
            Assert.NotNull(person);
            Assert.Equal(expectedFirstSync, person.DqtFirstSync);
            Assert.Equal(Clock.UtcNow, person.DqtLastSync);
        });
    }

    [Fact]
    public async Task Contact_DeletedRecord_DeletesPersonRecordFromDatabase()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePerson(b => b.WithSyncOverride(true));
        var contactId = createPersonResult.ContactId;
        var newItem = new NewOrUpdatedItem(ChangeType.NewOrUpdated, createPersonResult.Contact);

        var removedItem = new RemovedOrDeletedItem(ChangeType.RemoveOrDeleted, new EntityReference(Contact.EntityLogicalName, contactId));

        // Act
        await fixture.PublishChangedItemAndConsume(TrsDataSyncHelper.ModelTypes.Person, removedItem);

        // Assert
        await fixture.DbFixture.WithDbContext(async dbContext =>
        {
            var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == contactId);
            Assert.Null(person);
        });
    }
}
