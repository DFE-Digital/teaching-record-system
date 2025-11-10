using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.TestCommon.Infrastructure;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.TestCommon;

public sealed class DbHelper : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private Respawner? _respawner;
    private readonly SemaphoreSlim _schemaLock = new(1, 1);
    private bool _haveResetSchema;

    private DbHelper(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static DbHelper Instance { get; } = CreateInstance();

    public IDbContextFactory<TrsDbContext> DbContextFactory => _serviceProvider.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    private static DbHelper CreateInstance()
    {
        var configuration = TestConfiguration.GetConfiguration();

        var services = new ServiceCollection();
        services.AddDatabase(configuration.GetPostgresConnectionString());
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        return new DbHelper(serviceProvider);
    }

    public async Task InitializeAsync()
    {
        var schemaUpdated = await EnsureSchemaAsync();

        if (!schemaUpdated)
        {
            await using var dbContext = await DbContextFactory.CreateDbContextAsync();
            await SeedDbAsync(dbContext);
        }
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

    public async Task<bool> EnsureSchemaAsync()
    {
        await _schemaLock.WaitAsync();

        try
        {
            if (!_haveResetSchema)
            {
                await ResetSchemaAsync();

                await using var dbContext = await DbContextFactory.CreateDbContextAsync();
                await SeedDbAsync(dbContext);

                _haveResetSchema = true;
                return true;
            }
        }
        finally
        {
            _schemaLock.Release();
        }

        return false;
    }

    public async Task ResetSchemaAsync()
    {
        using var dbContext = await DbContextFactory.CreateDbContextAsync();

        var connection = dbContext.Database.GetDbConnection();
        var dbName = connection.Database;

        var cachedMigrationsVersionPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TeachingRecordSystem.Tests",
            $"{dbName}-dbversion.txt");

        var currentDbVersion = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(dbContext.Database.GenerateCreateScript())));

        if (currentDbVersion == GetPreviousMigrationsVersion())
        {
            return;
        }

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();

        WriteMigrationsVersion();

        await connection.OpenAsync();
        await EnsureRespawnerAsync(connection);

        string? GetPreviousMigrationsVersion() =>
            File.Exists(cachedMigrationsVersionPath) ? File.ReadAllText(cachedMigrationsVersionPath) : null;

        void WriteMigrationsVersion()
        {
            var directory = Path.GetDirectoryName(cachedMigrationsVersionPath)!;
            Directory.CreateDirectory(directory);
            File.WriteAllText(cachedMigrationsVersionPath, currentDbVersion);
        }
    }

    private async Task EnsureRespawnerAsync(DbConnection connection) =>
        _respawner = await Respawner.CreateAsync(
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

    private async Task SeedDbAsync(TrsDbContext dbContext)
    {
        await SeedLookupData.ResetTrainingProvidersAsync(dbContext);

        var existingUserIds = await dbContext.Set<UserBase>().Select(u => u.UserId).ToArrayAsync();

        void AddUserIfNotExists<T>(T user) where T : UserBase
        {
            if (!existingUserIds.Contains(user.UserId))
            {
                dbContext.Set<T>().Add(user);
            }
        }

        AddUserIfNotExists(SystemUser.Instance);
        AddUserIfNotExists(ApplicationUser.NpqApplicationUser);
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

    public void Dispose()
    {
        _schemaLock.Dispose();
        (_serviceProvider as IDisposable)?.Dispose();
    }
}
