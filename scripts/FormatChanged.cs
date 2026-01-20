#:package CliWrap@3.10.0

using CliWrap;
using CliWrap.Buffered;

async Task<IReadOnlyCollection<string>> GetChangedFilesAsync(string path)
{
    var result = await Cli.Wrap("git")
        .WithArguments(["status", "--porcelain", path])
        .ExecuteBufferedAsync();

    return result.StandardOutput
        .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(line => line.Substring(3))
        .Where(File.Exists)
        .ToList();
}

var changedTfFiles = await GetChangedFilesAsync("terraform/*.tf");
foreach (var tf in changedTfFiles)
{
    await Cli.Wrap("terraform")
        .WithArguments(["fmt", tf])
        .ExecuteAsync();
}

var changedCsFiles = await GetChangedFilesAsync("TeachingRecordSystem/**/*.cs");
changedCsFiles = changedCsFiles
    .Select(f => f.Replace("TeachingRecordSystem/", ""))
    .ToList();
if (changedCsFiles.Count > 0)
{
    var dotnetArgs = new List<string> { "format", "--no-restore", "--include" };
    dotnetArgs.AddRange(changedCsFiles);

    await Cli.Wrap("dotnet")
        .WithArguments(dotnetArgs)
        .WithWorkingDirectory("TeachingRecordSystem")
        .ExecuteAsync();
}
