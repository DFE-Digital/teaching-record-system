var testProjects = await Utils.GetAffectedTestProjectsAsync();

await using var stdOut = Console.OpenStandardOutput();
await using var stdErr = Console.OpenStandardError();

foreach (var testProject in testProjects)
{
    var dotnetArgs = new[] { "test" }.Concat(args);

    await (Cli.Wrap("dotnet")
            .WithArguments(dotnetArgs)
            .WithWorkingDirectory(Path.Combine("TeachingRecordSystem", "tests", testProject))
        | (stdOut, stdErr))
        .ExecuteAsync();
}
