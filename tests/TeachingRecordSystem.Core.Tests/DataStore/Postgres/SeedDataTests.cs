using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Tests.Services;

namespace TeachingRecordSystem.Core.Tests.DataStore.Postgres;

[Collection(nameof(DisableParallelization))]
public class SeedDataTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public async Task SeededData_SurvivesClearingTheDatabase()
    {
        // Arrange
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();

        var seededTableNames = TrsDbContext.SeededEntityTypes
            .Select(t => dbContext.Model.FindEntityType(t)!.GetTableName()!)
            .Distinct()
            .ToArray();

        // Act
        await DbHelper.Instance.ClearDataAsync();

        // Assert
        var emptyTableNames = new List<string>();

        foreach (var tableName in seededTableNames)
        {
            // The table names come from the EF model rather than from any input
#pragma warning disable EF1002
            var rowCount = await dbContext.Database
                .SqlQueryRaw<long>($"select count(*) as \"Value\" from \"{tableName}\"")
                .SingleAsync();
#pragma warning restore EF1002

            if (rowCount == 0)
            {
                emptyTableNames.Add(tableName);
            }
        }

        Assert.True(
            emptyTableNames.Count == 0,
            $"Seed data was removed by clearing the database: {string.Join(", ", emptyTableNames)}. " +
            "Tables holding seed data must either be ignored by Respawn or re-seeded by DbHelper.SeedDbAsync.");
    }
}
