using System.Text;
using Fixie;
using Fixie.Reports;
using static System.Environment;

namespace TeachingRecordSystem.TestFramework;

internal class GitHubReport : IReport, IHandler<ExecutionCompleted>, IHandler<TestFailed>
{
    private const string PassedIcon = "✅";
    private const string FailedIcon = "❌";

    private readonly TestEnvironment _environment;
    private readonly List<string> _failureMessages = new();

    public GitHubReport(TestEnvironment environment)
    {
        _environment = environment;
    }

    public async Task Handle(ExecutionCompleted message)
    {
        var assembly = Path.GetFileNameWithoutExtension(_environment.Assembly.Location);

        var lines = new StringBuilder();

        lines.AppendLine($"### {(message.Failed == 0 ? PassedIcon : FailedIcon)} {assembly}");
        lines.AppendLine();

        if (message.Total == 0)
        {
            lines.AppendLine("No tests found.");
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

            lines.AppendJoin(", ", parts);
            lines.Append($" in {message.Duration.TotalSeconds:0.00}s");
            lines.AppendLine();
        }

        if (_failureMessages.Count > 0)
        {
            lines.AppendLine();
            lines.AppendLine("#### Failed:");
            lines.AppendJoin(NewLine + NewLine, _failureMessages);
        }

        await AppendToJobSummary(lines.ToString());
    }

    public Task Handle(TestFailed message)
    {
        _failureMessages.Add($"##### `{message.TestCase}`\n```\n{message.Reason}\n```");

        return Task.CompletedTask;
    }

    private static async Task AppendToJobSummary(string summary)
    {
        if (GetEnvironmentVariable("GITHUB_STEP_SUMMARY") is string summaryFile)
        {
            await File.AppendAllTextAsync(summaryFile, summary);
        }
    }
}
