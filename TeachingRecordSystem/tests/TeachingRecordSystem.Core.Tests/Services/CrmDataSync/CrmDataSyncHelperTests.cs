using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Services.CrmDataSync;

namespace TeachingRecordSystem.Core.Tests.Services.CrmDataSync;

public class CrmDataSyncHelperTests
{
    private readonly DbFixture _dbFixture;
    private readonly FakeTrnGenerator _trnGenerator;

    public CrmDataSyncHelperTests(DbFixture dbFixture, FakeTrnGenerator trnGenerator)
    {
        _dbFixture = dbFixture;
        _trnGenerator = trnGenerator;
    }

    [Fact]
    public async Task SyncContact_NewRecord_WritesNewRowToDb()
    {
        // Arrange
        var clock = new TestableClock();
        var helper = new CrmDataSyncHelper(_dbFixture.GetDbContextFactory(), clock);

        var contactId = Guid.NewGuid();
        var trn = _trnGenerator.GenerateTrn();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = Faker.Identification.DateOfBirth();
        var email = Faker.Internet.Email();
        var nino = Faker.Identification.UkNationalInsuranceNumber();

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
            dfeta_StatedLastName = null
        };

        // Act
        await helper.SyncContact(contact);

        // Assert
        await _dbFixture.WithDbContext(async dbContext =>
        {
            var person = await dbContext.Persons.SingleAsync(p => p.DqtContactId == contact.Id);
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
            Assert.Equal(clock.UtcNow, person.DqtFirstSync);
            Assert.Equal(clock.UtcNow, person.DqtLastSync);
        });
    }

    [Fact]
    public async Task SyncContact_ExistingRecord_UpdatesExistingRowInDb()
    {
        // Arrange
        var clock = new TestableClock();
        var helper = new CrmDataSyncHelper(_dbFixture.GetDbContextFactory(), clock);

        var contactId = Guid.NewGuid();
        var trn = _trnGenerator.GenerateTrn();

        var initialFirstName = Faker.Name.First();
        var initialMiddleName = Faker.Name.Middle();
        var initialLastName = Faker.Name.Last();
        var initialDateOfBirth = Faker.Identification.DateOfBirth();
        var initialEmail = Faker.Internet.Email();
        var initialNino = Faker.Identification.UkNationalInsuranceNumber();
        var initialSyncTime = clock.UtcNow;

        await _dbFixture.WithDbContext(async dbContext =>
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
                DqtFirstSync = initialSyncTime,
                DqtLastSync = initialSyncTime
            });

            await dbContext.SaveChangesAsync();
        });

        clock.Advance();

        var newFirstName = Faker.Name.First();
        var newMiddleName = Faker.Name.Middle();
        var newLastName = Faker.Name.Last();
        var newDateOfBirth = Faker.Identification.DateOfBirth();
        var newEmail = Faker.Internet.Email();
        var newNino = Faker.Identification.UkNationalInsuranceNumber();

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
            dfeta_StatedFirstName = null,
            dfeta_StatedMiddleName = null,
            dfeta_StatedLastName = null
        };

        // Act
        await helper.SyncContact(contact);

        // Assert
        await _dbFixture.WithDbContext(async dbContext =>
        {
            var person = await dbContext.Persons.SingleAsync(p => p.DqtContactId == contact.Id);
            Assert.NotNull(person);
            Assert.Equal(newFirstName, person.FirstName);
            Assert.Equal(newMiddleName, person.MiddleName);
            Assert.Equal(newLastName, person.LastName);
            Assert.Equal(DateOnly.FromDateTime(newDateOfBirth), person.DateOfBirth);
            Assert.Equal(newEmail, person.EmailAddress);
            Assert.Equal(newNino, person.NationalInsuranceNumber);
            Assert.Equal((int)ContactState.Inactive, person.DqtState);
            Assert.Equal(initialSyncTime, person.DqtFirstSync);
            Assert.Equal(clock.UtcNow, person.DqtLastSync);
        });
    }
}
