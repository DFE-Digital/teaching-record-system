using System.Linq.Expressions;
using Hangfire;
using Hangfire.States;
using Npgsql;

namespace TeachingRecordSystem.Core.Jobs.Scheduling;

public class HangfireBackgroundJobScheduler(IBackgroundJobClient jobClient, NpgsqlDataSource dataSource) : IBackgroundJobScheduler
{
    public Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> expression) where T : notnull
    {
        var jobId = jobClient.Enqueue(expression);
        return Task.FromResult(jobId);
    }

    public Task<string> ContinueJobWithAsync<T>(string parentId, Expression<Func<T, Task>> expression) where T : notnull
    {
        var jobId = jobClient.ContinueJobWith(parentId, expression);
        return Task.FromResult(jobId);
    }

    public async Task WaitForJobToCompleteAsync(string jobId, CancellationToken cancellationToken)
    {
        await using var conn = dataSource.CreateConnection();
        await conn.OpenAsync(cancellationToken);

        await using var cmd = dataSource.CreateCommand();
        cmd.CommandText = "select statename from hangfire.job where id = @id";
        cmd.Parameters.AddWithValue("@id", long.Parse(jobId));

        while (!cancellationToken.IsCancellationRequested)
        {
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                // Job doesn't exist (yet)
                continue;
            }

            var stateName = reader.GetString(0);

            if (stateName == FailedState.StateName || stateName == SucceededState.StateName)
            {
                return;
            }

            await Task.Delay(100, cancellationToken);
        }
    }
}
