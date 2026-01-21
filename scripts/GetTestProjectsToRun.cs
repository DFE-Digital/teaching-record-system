#:package CliWrap@3.10.0

using CliWrap;
using CliWrap.Buffered;

if (args.Length > 1)
{
    Console.WriteLine("Usage: dotnet run GetTestProjectsToRun.cs <branch name?>");
    return 1;
}

var branchName = args.Length == 1 ? args[0] : "main";

var projectListFile = Path.GetTempFileName();

var incrementalistResult = await Cli.Wrap("dotnet")
    .WithArguments(new[]
    {
        "incrementalist",
        "run",
        "--file",
        projectListFile,
        "--branch",
        branchName,
        "--dir",
        "../",
        "--target-glob",
        "**/*Tests.csproj"
    })
    .ExecuteBufferedAsync();

var currentDirectory = Directory.GetCurrentDirectory();
var projectList = new HashSet<string>(File.ReadAllLines(projectListFile).Select(p => Path.GetFileNameWithoutExtension(p)));

// Check projects with Razor views individually as incrementalist doesn't currently know about them
var changedFilesResult = await Cli.Wrap("git")
    .WithArguments(new[]
    {
        "diff",
        "--name-only",
        $"origin/{branchName}"
    })
    .ExecuteBufferedAsync();
var changedFiles = changedFilesResult.StandardOutput
    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
    .ToList();

if (changedFiles.Any(f => f.Contains("TeachingRecordSystem.SupportUi")))
{
    projectList.Add("TeachingRecordSystem.SupportUi.Tests");
    projectList.Add("TeachingRecordSystem.SupportUi.EndToEndTests");
}

if (changedFiles.Any(f => f.Contains("TeachingRecordSystem.AuthorizeAccess")))
{
    projectList.Add("TeachingRecordSystem.AuthorizeAccess.Tests");
    projectList.Add("TeachingRecordSystem.AuthorizeAccess.EndToEndTests");
}

if (projectList.Count != 0)
{
    Console.WriteLine(string.Join("\n", projectList));
}

return 0;
