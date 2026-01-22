await using var stdOut = Console.OpenStandardOutput();
await using var stdErr = Console.OpenStandardError();

var changedFiles = Utils.GetChangedFiles();

var changedTfFiles = changedFiles.Where(path => path.StartsWith("terraform/") && path.EndsWith(".tf"));

foreach (var tf in changedTfFiles)
{
    await (Cli.Wrap("terraform")
            .WithArguments(["fmt", tf])
        | (stdOut, stdErr))
        .ExecuteAsync();
}

var changedCsFiles = changedFiles
    .Where(path => path.StartsWith("TeachingRecordSystem/") && path.EndsWith(".cs"))
    .Select(f => f.Replace("TeachingRecordSystem/", ""))
    .ToList();

if (changedCsFiles.Count > 0)
{
    var dotnetArgs = new List<string> { "format", "--no-restore", "--exclude", "src/TeachingRecordSystem.Core/DataStore/Postgres/Migrations", "--include" };
    dotnetArgs.AddRange(changedCsFiles);

    await (Cli.Wrap("dotnet")
            .WithArguments(dotnetArgs)
            .WithWorkingDirectory("TeachingRecordSystem")
        | (stdOut, stdErr))
        .ExecuteAsync();
}
