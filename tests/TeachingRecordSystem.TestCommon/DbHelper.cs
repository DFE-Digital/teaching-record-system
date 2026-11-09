using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.TestCommon.Infrastructure;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.TestCommon;

public sealed class DbHelper
{
    private readonly IServiceProvider _serviceProvider;

    private Respawner? _respawner;

    private DbHelper(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static DbHelper Instance { get; } = CreateInstance();

    public IDbContextFactory<TrsDbContext> DbContextFactory => _serviceProvider.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    private static DbHelper CreateInstance()
    {
        var services = new ServiceCollection();
        services.AddDatabase(TestDatabase.ConnectionString);
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        return new DbHelper(serviceProvider);
    }

    public async Task ClearDataAsync()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.Database.OpenConnectionAsync();
        var connection = dbContext.Database.GetDbConnection();
        await EnsureRespawnerAsync(connection);
        await _respawner!.ResetAsync(connection);
        await SeedDbAsync(dbContext);
    }

    private async Task EnsureRespawnerAsync(DbConnection connection) =>
        _respawner ??= await Respawner.CreateAsync(
            connection,
            new RespawnerOptions()
            {
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore = [
                    "mandatory_qualification_providers",
                    "establishment_sources",
                    "tps_establishment_types",
                    "alert_types",
                    "alert_categories",
                    "induction_exemption_reasons",
                    "route_to_professional_status_types",
                    "countries",
                    "training_subjects",
                    "degree_types",
                    "training_providers",
                    "support_task_types",
                    "induction_statuses",
                    "trn_ranges"
                ]
            });

    internal static async Task SeedDbAsync(TrsDbContext dbContext)
    {
        await SeedLookupData.EnsureTestTrainingProvidersAsync(dbContext);

        var existingUserIds = await dbContext.Set<UserBase>().Select(u => u.UserId).ToArrayAsync();

        void AddUserIfNotExists<T>(T user) where T : UserBase
        {
            if (!existingUserIds.Contains(user.UserId))
            {
                dbContext.Set<T>().Add(user);
            }
        }

        AddUserIfNotExists(SystemUser.Instance);
        AddUserIfNotExists(ApplicationUser.CapitaTpsImportUser);

        if (!await dbContext.Set<TrnRange>().AnyAsync())
        {
            dbContext.Set<TrnRange>().Add(new TrnRange
            {
                FromTrn = 8000000,
                ToTrn = 9999999,
                NextTrn = 8000000,
                IsExhausted = false
            });
        }

        await dbContext.SaveChangesAsync();
    }
}
