using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.TestCommon.Infrastructure;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.TestCommon.Database;

// The database every pooled test database is cloned from: migrated once, seeded with reference data once,
// then locked so nothing can connect to it (CREATE DATABASE ... TEMPLATE fails if any session is connected).
public sealed class TestDatabaseTemplate(
    string name,
    string schemaHash,
    IReadOnlyList<string> tablesToTruncate,
    IReadOnlyList<string> seededTables)
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

    private static readonly JsonSerializerOptions _seedDataSerializerOptions = new() { ReferenceHandler = ReferenceHandler.IgnoreCycles };

    // Bump when anything about how the template is built changes, so existing templates are not reused.
    private const int TemplateBuildVersion = 5;

    public string Name { get; } = name;

    public string SchemaHash { get; } = schemaHash;

    public string ResetStatement { get; } = BuildResetStatement(tablesToTruncate, seededTables);

    // Identifies everything that determines what state a pooled database should be in: the exact template it
    // was cloned from (whose name already covers the schema, the seed data, the build version and the registered
    // seeds) and the semantics of the reset applied between tests. A pooled database is only reused across runs
    // when both still match.
    //
    // The template name has to be in here, not just the schema: two test projects seed different data into
    // their own templates but share a schema, so keying on the schema alone had them reusing each other's
    // databases - and each other's seed snapshots - between runs.
    public string StateKey => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(Name + "|" + ResetStatement)))[..12].ToLowerInvariant();

    // Tables that aren't reference data but that the template put rows in hold both seeded rows and rows tests
    // create - the system users, the training providers RefreshTrainingProvidersJob adds to, the establishments
    // the migrations seed. Neither truncating nor preserving these is right, so they are truncated on reset and
    // refilled from a snapshot taken when the template was built, parents before children.
    private static string BuildResetStatement(IReadOnlyList<string> tablesToTruncate, IReadOnlyList<string> seededTables)
    {
        var statements = new List<string>();

        if (tablesToTruncate.Count > 0)
        {
            statements.Add(
                $"truncate table {string.Join(", ", tablesToTruncate.Select(t => $"\"{t}\""))} restart identity cascade");
        }

        foreach (var table in tablesToTruncate.Where(seededTables.Contains))
        {
            statements.Add($"insert into \"{table}\" select * from \"{SnapshotName(table)}\"");
        }

        return statements.Count == 0 ? "select 1" : string.Join("; ", statements);
    }

    public static async Task<TestDatabaseTemplate> EnsureAsync(
        TestDatabaseServer server,
        NpgsqlConnection lockConnection,
        IReadOnlyDictionary<string, Func<TrsDbContext, Task>> extraSeeds)
    {
        var model = ReadModel();
        // The registered seeds' keys are part of the name so a project adding or changing seed data gets a
        // fresh template rather than one built without it.
        var seedKey = extraSeeds.Count == 0
            ? "none"
            : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(",", extraSeeds.Keys))))[..8].ToLowerInvariant();

        var name = $"trs_tmpl_{model.SchemaHash}_{model.SeedDataHash}_v{TemplateBuildVersion}_{seedKey}";

        // Test projects that register the same seeds share a template, and are often started together, so only one
        // of them may build it; the others wait here and then find it already built.
        var buildLockName = $"{name}:build";
        await TestDatabaseServer.LockAsync(lockConnection, buildLockName);

        IReadOnlyList<string> seededTables;

        try
        {
            seededTables = await ReadSeededTablesAsync(lockConnection, name)
                ?? await BuildAsync(server, lockConnection, name, model.Tables, extraSeeds);

            // Held for as long as this process runs, so just drop-test-databases can tell the template is still
            // being cloned from. Shared, since every process using it holds one.
            await TestDatabaseServer.LockSharedAsync(lockConnection, name);
        }
        finally
        {
            await TestDatabaseServer.UnlockAsync(lockConnection, buildLockName);
        }

        return new TestDatabaseTemplate(name, model.SchemaHash, model.Tables, seededTables);
    }

    // The list of seeded tables is kept as a comment on the template, since nothing can connect to a template to
    // look. Returns null if there's no template by that name.
    private static async Task<IReadOnlyList<string>?> ReadSeededTablesAsync(NpgsqlConnection connection, string name)
    {
        var comment = await TestDatabaseServer.ExecuteScalarAsync<string>(
            connection,
            "select coalesce(shobj_description(oid, 'pg_database'), '') from pg_database where datname = @name",
            ("name", name));

        return comment?.Split(',', StringSplitOptions.RemoveEmptyEntries);
    }

    private static async Task<IReadOnlyList<string>> BuildAsync(
        TestDatabaseServer server,
        NpgsqlConnection lockConnection,
        string name,
        IReadOnlyList<string> tables,
        IReadOnlyDictionary<string, Func<TrsDbContext, Task>> extraSeeds)
    {
        // Build under a scratch name and only rename into place once it is complete, so a run that dies
        // mid-migration can't leave a half-built template that later runs would happily clone.
        var scratch = $"{name}_building_{Environment.ProcessId}";

        // Keeps just drop-test-databases off it while it's being built.
        await TestDatabaseServer.LockAsync(lockConnection, scratch);

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

        var seededTables = new List<string>();

        await using (var connection = new NpgsqlConnection(server.ConnectionStringFor(scratch)))
        {
            await connection.OpenAsync();

            // Found by looking rather than listed by hand, so that seeding a new table can't leave it emptied by
            // the first reset and never refilled.
            foreach (var table in tables)
            {
                if (await TestDatabaseServer.ExecuteScalarAsync<bool>(connection, $"select exists (select 1 from \"{table}\")"))
                {
                    seededTables.Add(table);
                }
            }

            foreach (var table in seededTables)
            {
                await ExecuteAsync(connection, $"create table \"{SnapshotName(table)}\" as table \"{table}\"");
            }

            // Table names are identifiers, so they can't contain the separator.
            await ExecuteAsync(connection, $"comment on database \"{scratch}\" is '{string.Join(",", seededTables)}'");
        }

        NpgsqlConnection.ClearAllPools();

        await server.ExecuteAsync($"drop database if exists \"{name}\" with (force)");
        await server.ExecuteAsync($"alter database \"{scratch}\" rename to \"{name}\"");
        await server.ExecuteAsync($"alter database \"{name}\" with allow_connections false is_template true");

        await TestDatabaseServer.UnlockAsync(lockConnection, scratch);

        return seededTables;
    }

    private static string SnapshotName(string table) => $"{table}__template_snapshot";

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
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

    // Generating the create script to hash it is slow enough to dominate a single-test run, so it, the seed data
    // hash and the table list are all cached against the identity of the assembly that defines the model.
    private static (string SchemaHash, string SeedDataHash, IReadOnlyList<string> Tables) ReadModel()
    {
        // Keyed on the model assembly *and* the table classifications below, since those decide which
        // tables end up in the list. Keying on the assembly alone silently serves a stale list when only
        // the classifications change.
        var mvid = typeof(TrsDbContext).Assembly.ManifestModule.ModuleVersionId;
        var classification = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            string.Join(",", _referenceTables) + "|" + TemplateBuildVersion)))[..8];
        var cachePath = Path.Combine(Path.GetTempPath(), $"trs-tests-schema-{mvid:N}-{classification}.txt");

        if (File.Exists(cachePath))
        {
            var cached = File.ReadAllLines(cachePath);
            if (cached.Length > 2)
            {
                return (cached[0], cached[1], cached[2..]);
            }
        }

        using var dbContext = TrsDbContext.Create("Host=localhost;Database=ignored");

        var schemaHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(dbContext.Database.GenerateCreateScript())))[..12].ToLowerInvariant();

        var seedDataHash = HashSeedData();

        var tables = GetTablesInDependencyOrder(dbContext.Model)
            .Where(t => !_referenceTables.Contains(t))
            .ToArray();

        File.WriteAllLines(cachePath, [schemaHash, seedDataHash, .. tables]);
        return (schemaHash, seedDataHash, tables);
    }

    // TrsDbContext writes its seed data as part of migrating, so it isn't in the create script; without hashing it
    // separately, changing the seed data alone would leave every existing template in use with the old rows.
    // The seed sets are the private static List<T> factories in TrsDbContext.Seeding.cs.
    private static string HashSeedData()
    {
        var seedSets = typeof(TrsDbContext)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(m => m.GetParameters().Length == 0 &&
                m.ReturnType.IsGenericType &&
                m.ReturnType.GetGenericTypeDefinition() == typeof(List<>))
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .Select(m => new { m.Name, Rows = m.Invoke(null, null) })
            .ToArray();

        if (seedSets.Length == 0)
        {
            throw new InvalidOperationException($"Could not find the seed data on {nameof(TrsDbContext)} to hash.");
        }

        var json = JsonSerializer.SerializeToUtf8Bytes(seedSets, _seedDataSerializerOptions);

        return Convert.ToHexString(SHA256.HashData(json))[..8].ToLowerInvariant();
    }

    // Principal tables before the tables that reference them, so the snapshots can be put back in order after a
    // truncate without tripping a foreign key.
    private static IEnumerable<string> GetTablesInDependencyOrder(IModel model)
    {
        var dependencies = model.GetEntityTypes()
            .Where(t => t.GetTableName() is not null)
            .GroupBy(t => t.GetTableName()!)
            .ToDictionary(
                g => g.Key,
                g => g.SelectMany(t => t.GetForeignKeys())
                    .Select(fk => fk.PrincipalEntityType.GetTableName())
                    .OfType<string>()
                    .Where(principal => principal != g.Key)
                    .Distinct()
                    .Order(StringComparer.Ordinal)
                    .ToArray());

        var ordered = new List<string>();
        var visited = new HashSet<string>();

        foreach (var table in dependencies.Keys.Order(StringComparer.Ordinal))
        {
            Visit(table);
        }

        return ordered;

        void Visit(string table)
        {
            // Adding before recursing means a cycle ends rather than recursing forever; no seeded table is in one.
            if (!visited.Add(table))
            {
                return;
            }

            foreach (var principal in dependencies.GetValueOrDefault(table, []))
            {
                Visit(principal);
            }

            ordered.Add(table);
        }
    }
}
