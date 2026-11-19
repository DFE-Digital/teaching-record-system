using System.CommandLine;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

public class FilesTests(IServiceProvider services) : CommandTestBase(services)
{
    [Fact]
    public void DownloadEwcWales_IsRegisteredUnderFilesCommand()
    {
        // Arrange & Act
        var filesCommand = Commands.CreateFilesCommand(Configuration);

        // Assert
        Assert.Equal("files", filesCommand.Name);
        Assert.Contains(filesCommand.Subcommands, c => c.Name == "download-ewc-wales");
    }

    [Fact]
    public void DownloadEwcWales_MissingOutputDirectoryOption_ReturnsParseError()
    {
        // Arrange
        var command = GetSubcommand(Configuration);

        // Act
        var parseResult = command.Parse(["--storage-account-name", "account", "--storage-access-key", "key"]);

        // Assert
        Assert.Contains(parseResult.Errors, e => e.Message == "Option '--output-directory' is required.");
    }

    [Fact]
    public void DownloadEwcWales_StorageOptionsNotConfiguredOrProvided_ReturnsParseErrors()
    {
        // Arrange
        var command = GetSubcommand(new ConfigurationBuilder().Build());

        // Act
        var parseResult = command.Parse(["--output-directory", "out"]);

        // Assert
        Assert.Contains(parseResult.Errors, e => e.Message == "Option '--storage-account-name' is required.");
        Assert.Contains(parseResult.Errors, e => e.Message == "Option '--storage-access-key' is required.");
    }

    [Fact]
    public void DownloadEwcWales_StorageOptionsConfigured_DoesNotRequireThemOnTheCommandLine()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["SftpStorageName"] = "account",
                    ["SftpStorageAccessKey"] = "key"
                })
            .Build();

        var command = GetSubcommand(configuration);

        // Act
        var parseResult = command.Parse(["--output-directory", "out"]);

        // Assert
        Assert.Empty(parseResult.Errors);
        Assert.Equal("account", parseResult.GetValue<string>("--storage-account-name"));
        Assert.Equal("key", parseResult.GetValue<string>("--storage-access-key"));
    }

    [Fact]
    public void DownloadEwcWales_FileOptionIsOptional()
    {
        // Arrange
        var command = GetSubcommand(Configuration);

        // Act
        var parseResult = command.Parse(
            ["--output-directory", "out", "--storage-account-name", "account", "--storage-access-key", "key", "--file", "ewc-trns.csv"]);

        // Assert
        Assert.Empty(parseResult.Errors);
        Assert.Equal("ewc-trns.csv", parseResult.GetValue<string?>("--file"));
    }

    [Fact]
    public void ArchiveEwcWales_IsRegisteredUnderFilesCommand()
    {
        var filesCommand = Commands.CreateFilesCommand(Configuration);

        Assert.Contains(filesCommand.Subcommands, c => c.Name == "archive-ewc-wales");
    }

    [Fact]
    public void ArchiveEwcWales_MissingFileOption_ReturnsParseError()
    {
        var command = GetArchiveSubcommand(Configuration);

        var parseResult = command.Parse(["--storage-account-name", "account", "--storage-access-key", "key"]);

        Assert.Contains(parseResult.Errors, e => e.Message == "Option '--file' is required.");
    }

    [Fact]
    public void ArchiveEwcWales_StorageOptionsNotConfiguredOrProvided_ReturnsParseErrors()
    {
        var command = GetArchiveSubcommand(new ConfigurationBuilder().Build());

        var parseResult = command.Parse(["--file", "ewc-trns.csv"]);

        Assert.Contains(parseResult.Errors, e => e.Message == "Option '--storage-account-name' is required.");
        Assert.Contains(parseResult.Errors, e => e.Message == "Option '--storage-access-key' is required.");
    }

    [Fact]
    public void ArchiveEwcWales_AllOptionsProvided_ParsesSuccessfully()
    {
        var command = GetArchiveSubcommand(Configuration);

        var parseResult = command.Parse(
            ["--file", "ewc-trns.csv", "--storage-account-name", "account", "--storage-access-key", "key"]);

        Assert.Empty(parseResult.Errors);
        Assert.Equal("ewc-trns.csv", parseResult.GetValue<string>("--file"));
    }

    private static Command GetArchiveSubcommand(IConfiguration configuration) =>
        Commands.CreateFilesCommand(configuration).Subcommands.Single(c => c.Name == "archive-ewc-wales");

    private static Command GetSubcommand(IConfiguration configuration) =>
        Commands.CreateFilesCommand(configuration).Subcommands.Single(c => c.Name == "download-ewc-wales");
}

