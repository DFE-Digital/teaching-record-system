using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TeachingRecordSystem.Core.Tests.DataStore.Postgres;

public class DbContextTests(DbFixture dbFixture)
{
    [Fact]
    public Task NoOutstandingMigrations() => dbFixture.WithDbContextAsync(dbContext =>
    {
        var migrationsAssembly = dbContext.GetService<IMigrationsAssembly>();

        if (migrationsAssembly.ModelSnapshot is not null)
        {
            var snapshotModel = migrationsAssembly.ModelSnapshot.Model;

            if (snapshotModel is IMutableModel mutableModel)
            {
                snapshotModel = mutableModel.FinalizeModel();
            }


            snapshotModel = dbContext.GetService<IModelRuntimeInitializer>().Initialize(snapshotModel);

            var migratedModel = snapshotModel.GetRelationalModel();
            var contextModel = dbContext.GetService<IDesignTimeModel>().Model.GetRelationalModel();
            var differences = dbContext.GetService<IMigrationsModelDiffer>().GetDifferences(
                migratedModel,
                contextModel);
            var hasDifferences = dbContext.GetService<IMigrationsModelDiffer>().HasDifferences(
                snapshotModel.GetRelationalModel(),
                dbContext.GetService<IDesignTimeModel>().Model.GetRelationalModel());

            Assert.False(hasDifferences, "DbContext has pending changes not covered by a migration.");
        }

        return Task.CompletedTask;
    });
}
