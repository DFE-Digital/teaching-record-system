using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

//using TeachingRecordSystem.Core.Events.Models;
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
        var updatedSchema = await EnsureSchemaAsync();

        if (updatedSchema)
        {
            await using var dbContext = await DbContextFactory.CreateDbContextAsync();
            await SeedLookupData.ResetTrainingProvidersAsync(dbContext);
        }
    }

    public async Task ClearDataAsync()
    {
        using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.Database.OpenConnectionAsync();
        var connection = dbContext.Database.GetDbConnection();
        await EnsureRespawnerAsync(connection);
        await _respawner!.ResetAsync(connection);

        // Ensure we have the System User around
        dbContext.Set<SystemUser>().Add(SystemUser.Instance);
        dbContext.Set<ApplicationUser>().Add(ApplicationUser.NpqApplicationUser);
        dbContext.Set<ApplicationUser>().Add(ApplicationUser.CapitaTpsImportUser);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> EnsureSchemaAsync()
    {
        await _schemaLock.WaitAsync();

        try
        {
            if (!_haveResetSchema)
            {
                await ResetSchemaAsync();
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
            await ClearDataAsync();
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

    public async Task DeleteAllPersonsAsync()
    {
        using var dbContext = await DbContextFactory.CreateDbContextAsync();

        await dbContext.Database.ExecuteSqlAsync(
            $"""
             delete from support_tasks where person_id is not null;
             update persons set merged_with_person_id = null, source_application_user_id = null, source_trn_request_id = null;
             delete from trn_request_metadata where resolved_person_id is not null;
             delete from previous_names;
             delete from persons;
             """);
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
                    "induction_statuses"
                ]
            });

    public void Dispose()
    {
        _schemaLock.Dispose();
        (_serviceProvider as IDisposable)?.Dispose();
    }
}
