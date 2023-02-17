using Microsoft.EntityFrameworkCore;
using QualifiedTeachersApi.DataStore.Sql;
using Respawn;

namespace QualifiedTeachersApi.Tests;

public class DbHelper
{
    private readonly string _connectionString;
    private Checkpoint _checkpoint;

    public DbHelper(string connectionString)
    {
        _connectionString = connectionString;
        CreateCheckpoint();
    }

    public async Task ClearData()
    {
        using var dbContext = new DqtContext(_connectionString);
        await dbContext.Database.OpenConnectionAsync();
        await _checkpoint.Reset(dbContext.Database.GetDbConnection());
    }

    public async Task ResetSchema()
    {
        using var dbContext = new DqtContext(_connectionString);
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();

        CreateCheckpoint();
    }

    private void CreateCheckpoint() => _checkpoint = new Checkpoint()
    {
        DbAdapter = DbAdapter.Postgres
    };
}
