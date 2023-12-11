using Microsoft.EntityFrameworkCore;
using Microsoft.Xrm.Sdk;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.Services.TrsDataSync;

public class TrsDataSyncServiceTests : IClassFixture<TrsDataSyncServiceFixture>
{
    private readonly TrsDataSyncServiceFixture _fixture;
    private readonly FakeTrnGenerator _trnGenerator;

    public TrsDataSyncServiceTests(TrsDataSyncServiceFixture fixture, FakeTrnGenerator trnGenerator)
    {
        _fixture = fixture;
        _trnGenerator = trnGenerator;
    }

    [Fact]
    public async Task ProcessChangesForEntityType_WritesNewRecordToDatabase()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var trn = _trnGenerator.GenerateTrn();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = Faker.Identification.DateOfBirth();
        var email = Faker.Internet.Email();
        var nino = Faker.Identification.UkNationalInsuranceNumber();
        var created = _fixture.Clock.UtcNow;
        var modified = _fixture.Clock.Advance();

        var contact = new Contact()
        {
            ContactId = contactId,
            dfeta_TRN = trn,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            BirthDate = dateOfBirth,
            EMailAddress1 = email,
            dfeta_NINumber = nino,
            StateCode = ContactState.Active,
            dfeta_StatedFirstName = null,
            dfeta_StatedMiddleName = null,
            dfeta_StatedLastName = null,
            CreatedOn = created,
            ModifiedOn = modified
        };

        var newItem = new NewOrUpdatedItem(ChangeType.NewOrUpdated, contact);

        // Act
        await _fixture.PublishChangedItemAndConsume(newItem);

        // Assert
        await _fixture.DbFixture.WithDbContext(async dbContext =>
        {
            var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == contact.Id);
            Assert.NotNull(person);
            Assert.Equal(contactId, person.PersonId);
            Assert.Equal(contactId, person.DqtContactId);
            Assert.Equal(trn, person.Trn);
            Assert.Equal(firstName, person.FirstName);
            Assert.Equal(middleName, person.MiddleName);
            Assert.Equal(lastName, person.LastName);
            Assert.Equal(DateOnly.FromDateTime(dateOfBirth), person.DateOfBirth);
            Assert.Equal(email, person.EmailAddress);
            Assert.Equal(nino, person.NationalInsuranceNumber);
            Assert.Equal((int)ContactState.Active, person.DqtState);
            Assert.Equal(created, person.DqtCreatedOn);
            Assert.Equal(modified, person.DqtModifiedOn);
            Assert.Equal(_fixture.Clock.UtcNow, person.DqtFirstSync);
            Assert.Equal(_fixture.Clock.UtcNow, person.DqtLastSync);
        });
    }

    [Fact]
    public async Task ProcessChangesForEntityType_WritesUpdatedRecordToDatabase()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var trn = _trnGenerator.GenerateTrn();

        var initialFirstName = Faker.Name.First();
        var initialMiddleName = Faker.Name.Middle();
        var initialLastName = Faker.Name.Last();
        var initialDateOfBirth = Faker.Identification.DateOfBirth();
        var initialEmail = Faker.Internet.Email();
        var initialNino = Faker.Identification.UkNationalInsuranceNumber();
        var initialSyncTime = _fixture.Clock.UtcNow;
        var created = _fixture.Clock.UtcNow;
        var initialModified = _fixture.Clock.UtcNow;

        await _fixture.DbFixture.WithDbContext(async dbContext =>
        {
            dbContext.Persons.Add(new()
            {
                PersonId = contactId,
                Trn = trn,
                FirstName = initialFirstName,
                MiddleName = initialMiddleName,
                LastName = initialLastName,
                DateOfBirth = DateOnly.FromDateTime(initialDateOfBirth),
                EmailAddress = initialEmail,
                NationalInsuranceNumber = initialNino,
                DqtContactId = contactId,
                DqtState = (int)ContactState.Active,
                DqtCreatedOn = created,
                DqtModifiedOn = initialModified,
                DqtFirstSync = initialSyncTime,
                DqtLastSync = initialSyncTime
            });

            await dbContext.SaveChangesAsync();
        });

        _fixture.Clock.Advance();

        var newFirstName = Faker.Name.First();
        var newMiddleName = Faker.Name.Middle();
        var newLastName = Faker.Name.Last();
        var newDateOfBirth = Faker.Identification.DateOfBirth();
        var newEmail = Faker.Internet.Email();
        var newNino = Faker.Identification.UkNationalInsuranceNumber();
        var newModified = _fixture.Clock.UtcNow;

        var contact = new Contact()
        {
            ContactId = contactId,
            dfeta_TRN = trn,
            FirstName = newFirstName,
            MiddleName = newMiddleName,
            LastName = newLastName,
            BirthDate = newDateOfBirth,
            EMailAddress1 = newEmail,
            dfeta_NINumber = newNino,
            StateCode = ContactState.Inactive,
            CreatedOn = created,
            ModifiedOn = newModified,
            dfeta_StatedFirstName = null,
            dfeta_StatedMiddleName = null,
            dfeta_StatedLastName = null
        };

        var updatedItem = new NewOrUpdatedItem(ChangeType.NewOrUpdated, contact);

        // Act
        await _fixture.PublishChangedItemAndConsume(updatedItem);

        // Assert
        await _fixture.DbFixture.WithDbContext(async dbContext =>
        {
            var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == contact.Id);
            Assert.NotNull(person);
            Assert.Equal(newFirstName, person.FirstName);
            Assert.Equal(newMiddleName, person.MiddleName);
            Assert.Equal(newLastName, person.LastName);
            Assert.Equal(DateOnly.FromDateTime(newDateOfBirth), person.DateOfBirth);
            Assert.Equal(newEmail, person.EmailAddress);
            Assert.Equal(newNino, person.NationalInsuranceNumber);
            Assert.Equal((int)ContactState.Inactive, person.DqtState);
            Assert.Equal(created, person.DqtCreatedOn);
            Assert.Equal(newModified, person.DqtModifiedOn);
            Assert.Equal(initialSyncTime, person.DqtFirstSync);
            Assert.Equal(_fixture.Clock.UtcNow, person.DqtLastSync);
        });
    }

    [Fact]
    public async Task ProcessChangesForEntityType_DeletesRemovedRecordFromDatabase()
    {
        // Arrange
        var contactId = Guid.NewGuid();
        var trn = _trnGenerator.GenerateTrn();

        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = Faker.Identification.DateOfBirth();
        var email = Faker.Internet.Email();
        var nino = Faker.Identification.UkNationalInsuranceNumber();
        var syncTime = _fixture.Clock.UtcNow;

        await _fixture.DbFixture.WithDbContext(async dbContext =>
        {
            dbContext.Persons.Add(new()
            {
                PersonId = contactId,
                Trn = trn,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = DateOnly.FromDateTime(dateOfBirth),
                EmailAddress = email,
                NationalInsuranceNumber = nino,
                DqtContactId = contactId,
                DqtState = (int)ContactState.Active,
                DqtFirstSync = syncTime,
                DqtLastSync = syncTime
            });

            await dbContext.SaveChangesAsync();
        });

        var removedItem = new RemovedOrDeletedItem(ChangeType.RemoveOrDeleted, new EntityReference(Contact.EntityLogicalName, contactId));

        // Act
        await _fixture.PublishChangedItemAndConsume(removedItem);

        // Assert
        await _fixture.DbFixture.WithDbContext(async dbContext =>
        {
            var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == contactId);
            Assert.Null(person);
        });
    }
}
