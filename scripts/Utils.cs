using LibGit2Sharp;

public static class Utils
{
    private const string DefaultBranch = "main";

    public static async Task<IReadOnlyCollection<string>> GetAffectedTestProjectsAsync(string targetBranch = DefaultBranch)
    {
        var projectListFile = Path.GetTempFileName();

        // TODO At some point we should use the Incrementalist library directly rather than shelling out to the CLI
        var incrementalistResult = await Cli.Wrap("dotnet")
            .WithArguments(new[]
            {
                "incrementalist",
                "run",
                "--file",
                projectListFile,
                "--branch",
                targetBranch
            })
            .ExecuteBufferedAsync();

        var projectList = new HashSet<string>(
            File.ReadAllLines(projectListFile)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(project => project.StartsWith("TeachingRecordSystem.", StringComparison.Ordinal)
                    && project.EndsWith("Tests", StringComparison.Ordinal)));

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
