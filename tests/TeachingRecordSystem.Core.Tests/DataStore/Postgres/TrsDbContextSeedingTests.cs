using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Tests.Services;

namespace TeachingRecordSystem.Core.Tests.DataStore.Postgres;

// Each test runs in a transaction that's rolled back, so the shared reference data is left untouched
public class TrsDbContextSeedingTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public Task SeedDataAsync_RunTwice_DoesNotChangeData() =>
        WithDbContextAsync(async dbContext =>
        {
            // Arrange
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            await dbContext.SeedDataAsync();
            var countriesBefore = await dbContext.Countries.AsNoTracking().OrderBy(c => c.CountryId).ToListAsync();

            // Act
            await dbContext.SeedDataAsync();

            // Assert
            var countriesAfter = await dbContext.Countries.AsNoTracking().OrderBy(c => c.CountryId).ToListAsync();
            Assert.Equal(
                countriesBefore.Select(c => (c.CountryId, c.Name, c.OfficialName, c.CitizenNames)),
                countriesAfter.Select(c => (c.CountryId, c.Name, c.OfficialName, c.CitizenNames)));
            var systemUser = await dbContext.Set<UserBase>().AsNoTracking().SingleAsync(u => u.UserId == SystemUser.SystemUserId);
            Assert.IsType<SystemUser>(systemUser);
            Assert.Equal(SystemUser.SystemUserName, systemUser.Name);
        });

    [Fact]
    public Task SeedDataAsync_SeededRowsEdited_RestoresSeededValues() =>
        WithDbContextAsync(async dbContext =>
        {
            // Arrange
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            await dbContext.Database.ExecuteSqlAsync($"update countries set name = 'Edited' where country_id = 'AF'");
            await dbContext.Database.ExecuteSqlAsync($"update users set name = 'Edited' where user_id = {SystemUser.SystemUserId}");

            // Act
            await dbContext.SeedDataAsync();

            // Assert
            var country = await dbContext.Countries.AsNoTracking().SingleAsync(c => c.CountryId == "AF");
            Assert.Equal("Afghanistan", country.Name);
            var systemUser = await dbContext.Set<UserBase>().AsNoTracking().SingleAsync(u => u.UserId == SystemUser.SystemUserId);
            Assert.IsType<SystemUser>(systemUser);
            Assert.Equal(SystemUser.SystemUserName, systemUser.Name);
        });
}
