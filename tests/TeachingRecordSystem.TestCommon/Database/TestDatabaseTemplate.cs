using System.Security.Cryptography;
using System.Text;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.TestCommon.Infrastructure;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.TestCommon.Database;

// The database every pooled test database is cloned from: migrated once, seeded with reference data once,
// then locked so nothing can connect to it (CREATE DATABASE ... TEMPLATE fails if any session is connected).
public sealed class TestDatabaseTemplate(string name, string schemaHash, IReadOnlyList<string> tablesToTruncate)
{
    // Reference data is cloned with the template and never truncated, so no test ever re-seeds it and
    // ReferenceDataCache stays valid across every database in the pool.
    private static readonly string[] _referenceTables =
    [
        "__EFMigrationsHistory",
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
        "support_task_types",
        "induction_statuses"
    ];

    // Tables that hold both rows the template seeded and rows tests create: the system users and the admin
    // user, and reference data that tests write to (RefreshTrainingProvidersJob inserts providers). Neither
    // truncating nor preserving these is right, so they are truncated on reset and refilled from a snapshot
    // taken when the template was built. Anything a project seeds via AddTemplateSeed belongs here.
    private static readonly string[] _seededTables = ["users", "training_providers", "trn_ranges"];

    // Bump when anything about how the template is built changes, so existing templates are not reused.
    private const int TemplateBuildVersion = 4;

    public string Name { get; } = name;

    public string SchemaHash { get; } = schemaHash;

    public string ResetStatement { get; } = BuildResetStatement(tablesToTruncate);

    // Identifies everything that determines what state a pooled database should be in: the exact template it
    // was cloned from (whose name already covers the schema, the build version and the registered seeds) and
    // the semantics of the reset applied between tests. A pooled database is only reused across runs when
    // both still match.
    //
    // The template name has to be in here, not just the schema: two test projects seed different data into
    // their own templates but share a schema, so keying on the schema alone had them reusing each other's
    // databases - and each other's seed snapshots - between runs.
    public string StateKey => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(Name + "|" + ResetStatement)))[..12].ToLowerInvariant();

    private static string BuildResetStatement(IReadOnlyList<string> tablesToTruncate)
    {
        var statements = new List<string>();

        if (tablesToTruncate.Count > 0)
        {
            statements.Add(
                $"truncate table {string.Join(", ", tablesToTruncate.Select(t => $"\"{t}\""))} restart identity cascade");
        }

        foreach (var table in _seededTables.Where(tablesToTruncate.Contains))
        {
            statements.Add($"insert into \"{table}\" select * from \"{SnapshotName(table)}\"");
        }

        return statements.Count == 0 ? "select 1" : string.Join("; ", statements);
    }

    public static async Task<TestDatabaseTemplate> EnsureAsync(
        TestDatabaseServer server,
        IReadOnlyDictionary<string, Func<TrsDbContext, Task>> extraSeeds)
    {
        var (schemaHash, tables) = ReadModel();
        // The registered seeds' keys are part of the name so a project adding or changing seed data gets a
        // fresh template rather than one built without it.
        var seedKey = extraSeeds.Count == 0
            ? "none"
            : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(",", extraSeeds.Keys))))[..8].ToLowerInvariant();

        var name = $"trs_tmpl_{schemaHash}_v{TemplateBuildVersion}_{seedKey}";

        var exists = await server.ExecuteScalarAsync<int?>(
            "select 1 from pg_database where datname = @name",
            ("name", name));

        if (exists is null)
        {
            await BuildAsync(server, name, extraSeeds);
        }

        return new TestDatabaseTemplate(name, schemaHash, tables);
    }

    private static async Task BuildAsync(
        TestDatabaseServer server,
        string name,
        IReadOnlyDictionary<string, Func<TrsDbContext, Task>> extraSeeds)
    {
        // Build under a scratch name and only rename into place once it is complete, so a run that dies
        // mid-migration can't leave a half-built template that later runs would happily clone.
        var scratch = $"{name}_building_{Environment.ProcessId}";

        await server.ExecuteAsync($"drop database if exists \"{scratch}\" with (force)");
        await server.ExecuteAsync($"create database \"{scratch}\"");

        await using (var dataSource = new NpgsqlDataSourceBuilder(server.ConnectionStringFor(scratch)).Build())
        {
            await using var dbContext = TrsDbContext.Create(dataSource);
            await dbContext.Database.MigrateAsync();
            await SeedAsync(dbContext);

            foreach (var seed in extraSeeds.Values)
            {
                await seed(dbContext);
            }
        }

        // Snapshot the mutable reference tables so a reset can put them back exactly as the template had them.
        foreach (var table in _seededTables)
        {
            await ExecuteOnAsync(server, scratch, $"create table \"{SnapshotName(table)}\" as table \"{table}\"");
        }

        NpgsqlConnection.ClearAllPools();

        await server.ExecuteAsync($"drop database if exists \"{name}\" with (force)");
        await server.ExecuteAsync($"alter database \"{scratch}\" rename to \"{name}\"");
        await server.ExecuteAsync($"alter database \"{name}\" with allow_connections false is_template true");
    }

    private static string SnapshotName(string table) => $"{table}__template_snapshot";

    private static async Task ExecuteOnAsync(TestDatabaseServer server, string database, string sql)
    {
        await using var connection = new NpgsqlConnection(server.ConnectionStringFor(database));
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task SeedAsync(TrsDbContext dbContext)
    {
        await SeedLookupData.EnsureTestTrainingProvidersAsync(dbContext);

        // TrsDbContext configures UseSeeding, so MigrateAsync may already have inserted these.
        var existingUserIds = await dbContext.Set<UserBase>().Select(u => u.UserId).ToArrayAsync();

        AddUserIfNotExists(SystemUser.Instance);
        AddUserIfNotExists(ApplicationUser.CapitaTpsImportUser);

        void AddUserIfNotExists<T>(T user) where T : UserBase
        {
            if (!existingUserIds.Contains(user.UserId))
            {
                dbContext.Set<T>().Add(user);
            }
        }

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

    // Generating the create script to hash it is slow enough to dominate a single-test run, so both it and
    // the table list are cached against the identity of the assembly that defines the model.
    private static (string Hash, IReadOnlyList<string> Tables) ReadModel()
    {
        // Keyed on the model assembly *and* the table classifications below, since those decide which
        // tables end up in the list. Keying on the assembly alone silently serves a stale list when only
        // the classifications change.
        var mvid = typeof(TrsDbContext).Assembly.ManifestModule.ModuleVersionId;
        var classification = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            string.Join(",", _referenceTables) + "|" + string.Join(",", _seededTables) + "|" + TemplateBuildVersion)))[..8];
        var cachePath = Path.Combine(Path.GetTempPath(), $"trs-tests-schema-{mvid:N}-{classification}.txt");

        if (File.Exists(cachePath))
        {
            var cached = File.ReadAllLines(cachePath);
            if (cached.Length > 1)
            {
                return (cached[0], cached[1..]);
            }
        }

        using var dbContext = TrsDbContext.Create("Host=localhost;Database=ignored");

        var hash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(dbContext.Database.GenerateCreateScript())))[..12].ToLowerInvariant();

        var tables = dbContext.Model.GetEntityTypes()
            .Select(t => t.GetTableName())
            .OfType<string>()
            .Distinct()
            .Where(t => !_referenceTables.Contains(t))
            .Order()
            .ToArray();

        File.WriteAllLines(cachePath, [hash, .. tables]);
        return (hash, tables);
    }
}
