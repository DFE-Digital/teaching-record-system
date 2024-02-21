using FakeXrmEasy.Extensions;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

public partial class TrsDataSyncHelperTests
{
    [Fact]
    public async Task SyncPerson_NewRecord_WritesNewRowToDb()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var contact = await CreatePersonEntity(contactId);

        // Act
        await Helper.SyncPerson(contact, ignoreInvalid: false);

        // Assert
        await AssertDatabasePersonMatchesEntity(contact);
    }

    [Fact]
    public async Task SyncPerson_ExistingRecord_UpdatesExistingRowInDb()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var existingEntity = await CreatePersonEntity(contactId);

        await Helper.SyncPerson(existingEntity, ignoreInvalid: false);
        var expectedFirstSync = Clock.UtcNow;

        Clock.Advance();
        var updatedEntity = await CreatePersonEntity(contactId, existingEntity);

        // Act
        await Helper.SyncPerson(updatedEntity, ignoreInvalid: false);

        // Assert
        await AssertDatabasePersonMatchesEntity(updatedEntity, expectedFirstSync);
    }

    [Fact]
    public async Task DeleteRecords_WithPerson_RemovesRowFromDb()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var existingEntity = await CreatePersonEntity(contactId);

        await Helper.SyncPerson(existingEntity, ignoreInvalid: false);

        // Act
        await Helper.DeleteRecords(TrsDataSyncHelper.ModelTypes.Person, new[] { contactId });

        // Assert
        await DbFixture.WithDbContext(async dbContext =>
        {
            var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == contactId);
            Assert.Null(person);
        });
    }

    [Fact]
    public async Task SyncPerson_AlreadyHaveNewerVersion_DoesNotUpdateDatabase()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var initialEntity = await CreatePersonEntity(contactId);

        Clock.Advance();
        var updatedEntity = await CreatePersonEntity(contactId, initialEntity);

        await Helper.SyncPerson(updatedEntity, ignoreInvalid: false);
        var expectedFirstSync = Clock.UtcNow;
        var expectedLastSync = Clock.UtcNow;

        // Act
        await Helper.SyncPerson(initialEntity, ignoreInvalid: false);

        // Assert
        await AssertDatabasePersonMatchesEntity(updatedEntity, expectedFirstSync, expectedLastSync);
    }

    private async Task AssertDatabasePersonMatchesEntity(
        Contact entity,
        DateTime? expectedFirstSync = null,
        DateTime? expectedLastSync = null)
    {
        await DbFixture.WithDbContext(async dbContext =>
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
        var trn = await TestData.GenerateTrn();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = Faker.Identification.DateOfBirth();
        var email = Faker.Internet.Email();
        var nino = Faker.Identification.UkNationalInsuranceNumber();

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

        return newContact;
    }
}
