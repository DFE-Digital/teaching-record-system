using Fixie;
using Fixie.Reports;
using static System.Environment;

namespace TeachingRecordSystem.TestFramework;

internal class GitHubReport : IReport, IHandler<ExecutionStarted>, IHandler<ExecutionCompleted>
{
    private readonly TestEnvironment _environment;

    public GitHubReport(TestEnvironment environment)
    {
        _environment = environment;
    }

    public async Task Handle(ExecutionStarted message)
    {
        var assembly = Path.GetFileNameWithoutExtension(_environment.Assembly.Location);

        await AppendToJobSummary($"## {assembly}{NewLine}{NewLine}");
    }

    public async Task Handle(ExecutionCompleted message)
    {
        string detail;

        if (message.Total == 0)
        {
            detail = "No tests found.";
        }
        else
        {
            var parts = new List<string>();

            if (message.Passed > 0)
            {
                parts.Add($"{message.Passed} passed");
            }

            if (message.Skipped > 0)
            {
                parts.Add($"{message.Skipped} skipped");
            }

            if (message.Failed > 0)
            {
                parts.Add($"{message.Failed} failed");
            }

            detail = string.Join(", ", parts);
        }

        await AppendToJobSummary($"{detail}{NewLine}{NewLine}");
    }

    private static async Task AppendToJobSummary(string summary)
    {
        if (GetEnvironmentVariable("GITHUB_STEP_SUMMARY") is string summaryFile)
        {
            await File.AppendAllTextAsync(summaryFile, summary);
        }
    }
}
