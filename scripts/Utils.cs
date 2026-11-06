using LibGit2Sharp;

public static class Utils
{
    private const string DefaultBranch = "main";

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
        // Incrementalist takes the repository root from the git directory, which for a git worktree is under
        // .git/worktrees. It then finds no solution and exits successfully without reporting any projects.
        if (!IsGitWorktree())
        {
            return await RunIncrementalistAsync(Environment.CurrentDirectory, targetBranch);
        }

        var cloneDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            await RunGitAsync(Environment.CurrentDirectory, "clone", "--quiet", "--shared", ".", cloneDirectory);
            await RunGitAsync(cloneDirectory, "fetch", "--quiet", "--update-head-ok", "origin", $"{targetBranch}:{targetBranch}");

            return await RunIncrementalistAsync(cloneDirectory, targetBranch);
        }
        finally
        {
            if (Directory.Exists(cloneDirectory))
            {
                Directory.Delete(cloneDirectory, recursive: true);
            }
        }
    }

    private static async Task<string[]> RunIncrementalistAsync(string workingDirectory, string targetBranch)
    {
        var projectListFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        // TODO At some point we should use the Incrementalist library directly rather than shelling out to the CLI
        await Cli.Wrap("dotnet")
            .WithArguments(new[]
            {
                "incrementalist",
                "run",
                "--file",
                projectListFile,
                "--branch",
                targetBranch
            })
            .WithWorkingDirectory(workingDirectory)
            .ExecuteBufferedAsync();

        // Incrementalist writes an empty file when nothing is affected, so a missing file means it didn't run the analysis.
        if (!File.Exists(projectListFile))
        {
            throw new InvalidOperationException($"Incrementalist did not analyze the solution in '{workingDirectory}'.");
        }

        var projectFiles = File.ReadAllLines(projectListFile);
        File.Delete(projectListFile);

        return projectFiles;
    }

    private static async Task RunGitAsync(string workingDirectory, params string[] arguments) =>
        await Cli.Wrap("git")
            .WithArguments(arguments)
            .WithWorkingDirectory(workingDirectory)
            .ExecuteBufferedAsync();

    // .git is a file rather than a directory in a git worktree
    private static bool IsGitWorktree() => File.Exists(Path.Combine(Environment.CurrentDirectory, ".git"));

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
