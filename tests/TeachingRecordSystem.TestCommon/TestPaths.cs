namespace TeachingRecordSystem.TestCommon;

internal static class TestPaths
{
    public static string RepositoryRoot { get; } = FindRepositoryRoot();

    public static string TestProjectName { get; } = FindTestProjectName();

    private static string FindRepositoryRoot() =>
        FindInAncestors(directory => File.Exists(Path.Combine(directory.FullName, "TeachingRecordSystem.slnx")) ? directory.FullName : null)
            ?? throw new InvalidOperationException($"Could not find the repository root from '{AppContext.BaseDirectory}'.");

    private static string FindTestProjectName() =>
        FindInAncestors(directory => directory.EnumerateFiles("*.csproj").FirstOrDefault() is FileInfo project ? Path.GetFileNameWithoutExtension(project.Name) : null)
            ?? throw new InvalidOperationException($"Could not find the test project from '{AppContext.BaseDirectory}'.");

    private static string? FindInAncestors(Func<DirectoryInfo, string?> select)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (select(directory) is string result)
            {
                return result;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
