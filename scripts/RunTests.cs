#!/usr/bin/env -S dotnet --
#:property PublishAot=false

using System.Text.Json;

// Runs a test project's classes across several processes, each of which gets its own database, so tests that have to
// run serially within a process run side by side overall. See docs/test-databases.md.

const int MinimumSecondsPerProcess = 10;

if (args.Length == 0 || args.Length % 2 == 0)
{
    Console.WriteLine("Usage: dotnet run scripts/RunTests.cs -- <project> [--configuration <name>] [--shard <n>] [--shards <n>] [--processes <n>]");
    return 1;
}

var project = args[0];
var options = args.Skip(1).Chunk(2).ToDictionary(pair => pair[0], pair => pair[1]);
var configuration = options.GetValueOrDefault("--configuration", "Debug");
var shard = int.Parse(options.GetValueOrDefault("--shard", "1"));
var shards = int.Parse(options.GetValueOrDefault("--shards", "1"));
var maxProcesses = int.Parse(options.GetValueOrDefault("--processes", "4"));

var projectDirectory = Path.Combine("tests", $"TeachingRecordSystem.{project}");
var outputDirectory = Path.GetFullPath(Path.Combine(projectDirectory, "bin", configuration, "net11.0"));
var testHost = Path.Combine(outputDirectory, $"TeachingRecordSystem.{project}{(OperatingSystem.IsWindows() ? ".exe" : "")}");

var listing = await Cli.Wrap(testHost)
    .WithArguments(["-list", "methods/json"])
    .WithWorkingDirectory(outputDirectory)
    .ExecuteBufferedAsync();

var methods = JsonSerializer.Deserialize<string[]>(listing.StandardOutput.Split('\n')[0])!;

var methodCounts = methods
    .GroupBy(method => method[..method.LastIndexOf('.')])
    .ToDictionary(group => group.Key, group => group.Count());

var timings = await ReadTimingsAsync(project);

// A class that has no timing yet is assumed to be as slow per test as the classes that have.
var timedMethods = methodCounts.Where(c => timings.ContainsKey(c.Key)).Sum(c => c.Value);
var secondsPerMethod = timedMethods > 0 ? timings.Where(t => methodCounts.ContainsKey(t.Key)).Sum(t => t.Value) / timedMethods : 1;

var classes = methodCounts
    .Select(c => new TestClass(c.Key, c.Value, timings.GetValueOrDefault(c.Key, c.Value * secondsPerMethod)))
    .ToArray();

var shardClasses = Split(classes, shards)[shard - 1];

if (shardClasses.Count == 0)
{
    Console.Error.WriteLine($"Shard {shard} of {shards} matched no test classes.");
    return 1;
}

// Every process pays to start its hosts, which isn't worth it for a small amount of work.
var shardSeconds = shardClasses.Sum(c => c.Seconds);
var processCount = Math.Clamp((int)(shardSeconds / MinimumSecondsPerProcess), 1, maxProcesses);
var processes = Split(shardClasses, processCount).Where(p => p.Count > 0).ToArray();

Console.WriteLine($"Running {shardClasses.Count} classes (about {shardSeconds:F0}s of tests) across {processes.Length} process(es).");

var exitCodes = await Task.WhenAll(processes.Select((classes, index) => RunProcessAsync(index + 1, classes)));

return exitCodes.All(code => code == 0) ? 0 : 1;

async Task<int> RunProcessAsync(int number, IReadOnlyCollection<TestClass> processClasses)
{
    var output = PipeTarget.ToDelegate(line => Console.WriteLine($"[{number}] {line}"));

    // A [Theory] expands to at least one test, so the method count is a valid lower bound. Passing it turns "the filter
    // matched nothing" into a failure rather than a green run of zero tests.
    // --test-modules runs the built test assembly without evaluating the project, which would need the restored
    // packages; the CI jobs that run tests only have the build output.
    string[] arguments =
    [
        "test",
        "--test-modules", $"TeachingRecordSystem.{project}.dll",
        "--root-directory", outputDirectory,
        "--hangdump",
        "--hangdump-timeout", "10m",
        "--report-xunit-trx",
        "--results-directory", Path.GetFullPath(Path.Combine(projectDirectory, "TestResults", $"process-{number}")),
        "--minimum-expected-tests", processClasses.Sum(c => c.Methods).ToString(),
        "--filter-class", .. processClasses.Select(c => c.Name)
    ];

    var result = await Cli.Wrap("dotnet")
        .WithArguments(arguments)
        .WithWorkingDirectory(outputDirectory)
        .WithStandardOutputPipe(output)
        .WithStandardErrorPipe(output)
        .WithValidation(CommandResultValidation.None)
        .ExecuteAsync();

    return result.ExitCode;
}

static async Task<Dictionary<string, double>> ReadTimingsAsync(string project)
{
    await using var file = File.OpenRead(Path.Combine("tests", "test-timings.json"));
    var timings = await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, double>>>(file);
    return timings!.GetValueOrDefault(project, []);
}

// Longest first, each to whichever group is lightest so far. Every shard has to arrive at the same answer,
// so ties are broken by name.
static List<TestClass>[] Split(IEnumerable<TestClass> classes, int groups)
{
    var result = Enumerable.Range(0, groups).Select(_ => new List<TestClass>()).ToArray();

    foreach (var testClass in classes.OrderByDescending(c => c.Seconds).ThenBy(c => c.Name, StringComparer.Ordinal))
    {
        result.MinBy(group => group.Sum(c => c.Seconds))!.Add(testClass);
    }

    return result;
}

record TestClass(string Name, int Methods, double Seconds);
