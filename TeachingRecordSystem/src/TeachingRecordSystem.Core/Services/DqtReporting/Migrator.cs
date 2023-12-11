#nullable enable
using System.Text;
using DbUp;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.ScriptProviders;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace TeachingRecordSystem.Core.Services.DqtReporting;

public class Migrator
{
    private readonly UpgradeEngine _upgradeEngine;
    private readonly string _connectionString;

    public Migrator(string connectionString, ILogger? logger = null)
    {
        var builder = DeployChanges.To
            .SqlDatabase(connectionString)
            .LogScriptOutput()
            .WithTransaction()
            .JournalToSqlTable("dbo", "__SchemaVersions")
            .WithScripts(new DqtReportingMigrationsScriptProvider());

        if (logger is not null)
        {
            builder.LogTo(new LoggerUpgradeLogWrapper(logger));
        }
        else
        {
            builder.LogToConsole();
        }

        _upgradeEngine = builder.Build();
        _connectionString = connectionString;
    }

    public void DropAllTables()
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        var cmd = new SqlCommand("sp_MSforeachtable", conn);
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.Add(new SqlParameter("@command1", "DROP TABLE ?"));
        cmd.ExecuteNonQuery();
    }

    public void MigrateDb()
    {
        var upgradeResult = _upgradeEngine.PerformUpgrade();

        if (!upgradeResult.Successful)
        {
            throw upgradeResult.Error;
        }
    }

    public void ResetJournal(string migrationName)
    {
        var upgradeResult = _upgradeEngine.MarkAsExecuted(migrationName);

        if (!upgradeResult.Successful)
        {
            throw upgradeResult.Error;
        }
    }

    private class LoggerUpgradeLogWrapper : IUpgradeLog
    {
        private readonly ILogger _logger;

        public LoggerUpgradeLogWrapper(ILogger logger)
        {
            _logger = logger;
        }

        public void WriteError(string format, params object[] args)
        {
            _logger.LogError(format, args);
        }

        public void WriteInformation(string format, params object[] args)
        {
            _logger.LogInformation(format, args);
        }

        public void WriteWarning(string format, params object[] args)
        {
            _logger.LogWarning(format, args);
        }
    }

    private class DqtReportingMigrationsScriptProvider : IScriptProvider
    {
        public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            var innerProvider = new EmbeddedScriptProvider(
                typeof(Migrator).Assembly,
                filter: name => name.Contains(".DqtReporting.Migrations.") && name.EndsWith(".sql"),
                Encoding.UTF8);

            var scripts = innerProvider.GetScripts(connectionManager);

            return scripts.Select(s => new SqlScript(Path.GetFileName(RemoveNamespaceFromScriptName(s.Name)), s.Contents));

            static string RemoveNamespaceFromScriptName(string name) =>
                name[(name.IndexOf("Migrations") + "Migrations".Length + 1)..];
        }
    }
}
