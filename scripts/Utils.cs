using LibGit2Sharp;

public static class Utils
{
    private const string DefaultBranch = "main";
    private const int NoProjectsAffectedExitCode = 166;

    public static async Task<IReadOnlyCollection<string>> GetAffectedTestProjectsAsync(string targetBranch = DefaultBranch)
    {
        var projectList = new HashSet<string>((await GetAffectedProjectFilesAsync(targetBranch)).Select(p => Path.GetFileNameWithoutExtension(p)));

        projectList.RemoveWhere(project => !project.EndsWith("Tests"));

        // Check projects with Razor views individually as incrementalist doesn't currently know about them
        var changedFiles = Utils.GetChangedFiles(targetBranch);

        if (changedFiles.Any(f => f.Contains("TeachingRecordSystem.SupportUi")))
        {
            projectList.Add("TeachingRecordSystem.SupportUi.Tests");
            projectList.Add("TeachingRecordSystem.EndToEndTests");
        }

        if (changedFiles.Any(f => f.Contains("TeachingRecordSystem.AuthorizeAccess")))
        {
            projectList.Add("TeachingRecordSystem.AuthorizeAccess.Tests");
            projectList.Add("TeachingRecordSystem.EndToEndTests");
        }

        return projectList;
    }

    private static async Task<string[]> GetAffectedProjectFilesAsync(string targetBranch)
    {
        var outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        var result = await Cli.Wrap("dotnet")
            .WithArguments(new[]
            {
                "tool",
                "run",
                "dotnet-affected",
                "--",
                "--from",
                targetBranch,
                "--filter-file-path",
                "TeachingRecordSystem.slnx",
                "--format",
                "text",
                "--output-dir",
                outputDirectory,
                "--output-name",
                "affected"
            })
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        if (result.ExitCode == NoProjectsAffectedExitCode)
        {
            return [];
        }

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"dotnet-affected failed with exit code {result.ExitCode}:\n{result.StandardOutput}{result.StandardError}");
        }

        var projectFiles = File.ReadAllLines(Path.Combine(outputDirectory, "affected.txt"));
        Directory.Delete(outputDirectory, recursive: true);

        return projectFiles;
    }

    public static IReadOnlyCollection<string> GetChangedFiles(string targetBranch = DefaultBranch)
    {
        var repository = new Repository(Environment.CurrentDirectory);

        var targetTree = repository.Branches[targetBranch].Tip.Tree;
        var changes = new HashSet<string>();

        var status = repository.RetrieveStatus();

        foreach (var staged in status.Staged)
        {
            changes.Add(staged.FilePath);
        }

        foreach (var unstaged in status.Modified.Concat(status.Added).Concat(status.Untracked))
        {
            changes.Add(unstaged.FilePath);
        }

        var branchDiff = repository.Diff.Compare<TreeChanges>(targetTree, repository.Head.Tip.Tree);
        foreach (var change in branchDiff)
        {
            changes.Add(change.Path);
        }

        return changes;
    }
}
