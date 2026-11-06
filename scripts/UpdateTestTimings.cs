#!/usr/bin/env -S dotnet --
#:property PublishAot=false

using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

// Updates tests/test-timings.json from the test results of a CI run. Projects the run didn't test keep their timings.

if (args.Length != 1)
{
    Console.WriteLine("Usage: dotnet run scripts/UpdateTestTimings.cs -- <run ID>");
    return 1;
}

var timingsPath = Path.Combine("tests", "test-timings.json");
var downloadDirectory = Directory.CreateTempSubdirectory("test-timings");

try
{
    await Cli.Wrap("gh")
        .WithArguments(["run", "download", args[0], "--pattern", "test-results-*", "--dir", downloadDirectory.FullName])
        .ExecuteAsync();

    var timings = JsonSerializer.Deserialize<SortedDictionary<string, SortedDictionary<string, double>>>(
        await File.ReadAllTextAsync(timingsPath))!;

    var resultsByProject = downloadDirectory.GetDirectories()
        .GroupBy(directory => Regex.Match(directory.Name, "^test-results-(.+)-[0-9]+$").Groups[1].Value);

    foreach (var results in resultsByProject)
    {
        var files = results.SelectMany(directory => directory.GetFiles("*.trx", SearchOption.AllDirectories));
        timings[results.Key] = new(GetSecondsByClass(files));
    }

    await File.WriteAllTextAsync(
        timingsPath,
        JsonSerializer.Serialize(timings, new JsonSerializerOptions { WriteIndented = true }) + "\n");

    return 0;
}
finally
{
    downloadDirectory.Delete(recursive: true);
}

static Dictionary<string, double> GetSecondsByClass(IEnumerable<FileInfo> files)
{
    var seconds = new Dictionary<string, double>();

    foreach (var file in files)
    {
        var document = XDocument.Load(file.FullName);

        var classesByTestId = document.Descendants().Where(e => e.Name.LocalName == "UnitTest").ToDictionary(
            test => test.Attribute("id")!.Value,
            test => test.Elements().Single(e => e.Name.LocalName == "TestMethod").Attribute("className")!.Value);

        foreach (var result in document.Descendants().Where(e => e.Name.LocalName == "UnitTestResult"))
        {
            if (result.Attribute("duration") is { } duration)
            {
                var className = classesByTestId[result.Attribute("testId")!.Value];
                seconds[className] = seconds.GetValueOrDefault(className) + TimeSpan.Parse(duration.Value).TotalSeconds;
            }
        }
    }

    return seconds.ToDictionary(pair => pair.Key, pair => Math.Round(pair.Value, 1));
}
