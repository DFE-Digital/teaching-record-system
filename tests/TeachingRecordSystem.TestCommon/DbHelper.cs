using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.TestCommon.Infrastructure;
using Testcontainers.PostgreSql;
using Establishment = TeachingRecordSystem.Core.DataStore.Postgres.Models.Establishment;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.TestCommon;

public sealed class DbHelper : IAsyncDisposable
{
    private const int DefaultTestContainersPostgresPort = 43007;
    private const string MaintenanceDatabaseName = "postgres";
    private static readonly string SeedingSourceFilePath =
        Path.Combine("src", "TeachingRecordSystem.Core", "DataStore", "Postgres", "TrsDbContext.Seeding.cs");

    // Reference data that SeedDbAsync writes rather than the migrations; it needs the same protection from Respawn as
    // the data seeded by TrsDbContext.
    private static readonly Type[] TestSeededEntityTypes = [typeof(TrainingProvider), typeof(TrnRange)];

    // Tests write rows of their own to these tables alongside the seed data, so they have to be cleared down like any
    // other; SeedDbAsync puts the seed data back afterwards.
    private static readonly Type[] ClearedAndReseededEntityTypes = [typeof(SystemUser), typeof(Establishment)];

    private readonly IServiceProvider _serviceProvider;
    private readonly PostgreSqlContainer? _postgresContainer;
    private readonly string _connectionString;

    private Respawner? _respawner;
    private readonly SemaphoreSlim _schemaLock = new(1, 1);
    private bool _haveResetSchema;

    private DbHelper(IServiceProvider serviceProvider, PostgreSqlContainer? postgresContainer, string connectionString)
    {
        _serviceProvider = serviceProvider;
        _postgresContainer = postgresContainer;
        _connectionString = connectionString;
    }

    public static DbHelper Instance { get; } = CreateInstance();

    public IDbContextFactory<TrsDbContext> DbContextFactory => _serviceProvider.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    public static string GetTestContainersConnectionString(int port) =>
        $"Host=localhost;Port={port};Database=trs;Username=postgres;Password=postgres;";

    public static int GetTestContainersPostgresPort(IConfiguration configuration) =>
        configuration.GetValue<int?>("TestContainersPostgresPort") ?? DefaultTestContainersPostgresPort;

    private static DbHelper CreateInstance()
    {
        var configuration = TestConfiguration.GetConfiguration();

        var connectionString = configuration.GetPostgresConnectionString();

        PostgreSqlContainer? postgresContainer = null;
        var useTestContainers = configuration.GetValue<bool>("UseTestContainers");
        if (useTestContainers)
        {
            postgresContainer = new PostgreSqlBuilder("postgres:17")
                .WithDatabase("trs")
                .WithReuse(true)
                .WithPortBinding(GetTestContainersPostgresPort(configuration), 5432)
                .Build();
        }

        var services = new ServiceCollection();
        services.AddDatabase(connectionString);
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

        return new DbHelper(serviceProvider, postgresContainer, connectionString);
    }

    public async Task InitializeAsync()
    {
        if (_postgresContainer != null)
        {
            await _postgresContainer.StartAsync();
        }

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

        var currentDbVersion = GetDbVersion(dbContext);

        // Deliberately the connection string we were configured with rather than the one EF hands back, which has had
        // the password stripped out of it and so can't be used to open a connection of our own.
        if (currentDbVersion == await GetStoredDbVersionAsync(_connectionString))
        {
            return;
        }

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();

        await connection.OpenAsync();
        await StoreDbVersionAsync(connection, currentDbVersion);
        await EnsureRespawnerAsync(connection);
    }

    // The version is stored on the database itself rather than alongside the repository so that it can never describe a
    // database other than the one the tests are about to use; dropping or replacing the database takes the version with it.
    private static async Task<string?> GetStoredDbVersionAsync(string connectionString)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = connectionStringBuilder.Database!;
        connectionStringBuilder.Database = MaintenanceDatabaseName;

        // pg_database is a shared catalog so the comment is readable without connecting to the database it belongs to;
        // connecting to it here would leave a pooled connection open and DROP DATABASE below would fail.
        await using var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "select shobj_description(oid, 'pg_database') from pg_database where datname = @databaseName";
        command.Parameters.AddWithValue("databaseName", databaseName);

        return await command.ExecuteScalarAsync() as string;
    }

    private static async Task StoreDbVersionAsync(DbConnection connection, string version)
    {
        await using var command = connection.CreateCommand();
        // COMMENT ON doesn't accept parameters; the version is always the hex output of a hash so it's safe to inline.
        command.CommandText = $"comment on database \"{connection.Database}\" is '{version}'";
        await command.ExecuteNonQueryAsync();
    }

    private static string GetDbVersion(TrsDbContext dbContext)
    {
        // The seed data is only written when EF migrates the database, so changing it without also changing the schema
        // would otherwise leave every existing test database with the old reference data.
        var seedingSourcePath = Path.Combine(TestPaths.RepositoryRoot, SeedingSourceFilePath);

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(Encoding.UTF8.GetBytes(dbContext.Database.GenerateCreateScript()));
        hash.AppendData(File.ReadAllBytes(seedingSourcePath));

        return Convert.ToHexString(hash.GetHashAndReset());
    }

    // Creating a Respawner interrogates the schema to work out what to delete and in what order, which is worth doing
    // once rather than before every test; the schema doesn't change again once it's been reset.
    private async Task EnsureRespawnerAsync(DbConnection connection)
    {
        if (_respawner is not null)
        {
            return;
        }

        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions()
            {
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore = GetTablesToIgnore()
            });
    }

    private Table[] GetTablesToIgnore()
    {
        using var dbContext = DbContextFactory.CreateDbContext();

        return TrsDbContext.SeededEntityTypes
            .Concat(TestSeededEntityTypes)
            .Except(ClearedAndReseededEntityTypes)
            .Select(t => dbContext.Model.FindEntityType(t)!.GetTableName()!)
            .Distinct()
            .Select(t => new Table(t))
            .ToArray();
    }

    private async Task SeedDbAsync(TrsDbContext dbContext)
    {
        await SeedLookupData.EnsureTestTrainingProvidersAsync(dbContext);

        foreach (var entityType in ClearedAndReseededEntityTypes)
        {
            await dbContext.SeedDataForEntityTypeAsync(entityType);
        }

        var existingUserIds = await dbContext.Set<UserBase>().Select(u => u.UserId).ToArrayAsync();

        void AddUserIfNotExists<T>(T user) where T : UserBase
        {
            if (!existingUserIds.Contains(user.UserId))
            {
                dbContext.Set<T>().Add(user);
            }
        }

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

    public async ValueTask DisposeAsync()
    {
        _schemaLock.Dispose();
        (_serviceProvider as IDisposable)?.Dispose();

        if (_postgresContainer is not null)
        {
            await _postgresContainer.DisposeAsync();
        }
    }
}
