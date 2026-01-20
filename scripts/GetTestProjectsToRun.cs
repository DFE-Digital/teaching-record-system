if (args.Length > 1)
{
    Console.WriteLine("Usage: dotnet run GetTestProjectsToRun.cs <branch name?>");
    return 1;
}

var branchName = args.Length == 1 ? args[0] : "main";

var projectList = (await Utils.GetAffectedTestProjectsAsync(branchName))
    .Select(project => project[("TeachingRecordSystem.").Length..])
    .ToArray();

if (projectList.Length != 0)
{
    Console.WriteLine(string.Join("\n", projectList));
}

return 0;
