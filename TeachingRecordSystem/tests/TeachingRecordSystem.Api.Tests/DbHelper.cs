﻿using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Respawn;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.Tests;

public class DbHelper
{
    private Respawner? _respawner;
    private readonly SemaphoreSlim _schemaLock = new(1, 1);
    private bool _haveResetSchema = false;

    public DbHelper(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }

    public async Task ClearData()
    {
        using var dbContext = TrsDbContext.Create(ConnectionString);
        await dbContext.Database.OpenConnectionAsync();
        var connection = dbContext.Database.GetDbConnection();
        await EnsureRespawner(connection);
        await _respawner!.ResetAsync(connection);
    }

    public async Task EnsureSchema()
    {
        await _schemaLock.WaitAsync();

        try
        {
            if (!_haveResetSchema)
            {
                await ResetSchema();
                _haveResetSchema = true;
            }
        }
        finally
        {
            _schemaLock.Release();
        }
    }

    public async Task ResetSchema()
    {
        using var dbContext = TrsDbContext.Create(ConnectionString);

        var connection = dbContext.Database.GetDbConnection();
        var dbName = connection.Database;

        var cachedMigrationsVersionPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TeachingRecordSystem.Api.Tests",
            $"{dbName}-dbversion.txt");

        var currentDbVersion = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(dbContext.Database.GenerateCreateScript())));

        if (currentDbVersion == GetPreviousMigrationsVersion())
        {
            await ClearData();
            return;
        }

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();

        WriteMigrationsVersion();

        await connection.OpenAsync();
        await EnsureRespawner(connection);

        string? GetPreviousMigrationsVersion() =>
            File.Exists(cachedMigrationsVersionPath) ? File.ReadAllText(cachedMigrationsVersionPath) : null;

        void WriteMigrationsVersion()
        {
            var directory = Path.GetDirectoryName(cachedMigrationsVersionPath)!;
            Directory.CreateDirectory(directory);
            File.WriteAllText(cachedMigrationsVersionPath, currentDbVersion);
        }
    }

    private async Task EnsureRespawner(DbConnection connection) =>
        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions()
            {
                DbAdapter = DbAdapter.Postgres
            });
}
