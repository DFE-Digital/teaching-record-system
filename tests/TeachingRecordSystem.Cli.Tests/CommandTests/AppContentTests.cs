using System.CommandLine;
using System.Text.Json;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

public class AppContentTests(IServiceProvider services) : CommandTestBase(services)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public async Task Export_WithValidUserId_ExportsAppContentToStdout()
    {
        // Arrange
        var applicationUser = await CreateApplicationUserWithAppContentAsync();
        var command = GetExportCommand();

        // Act
        var parseResult = command.Parse($"--user-id {applicationUser.UserId}");
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Export_WithValidUserIdAndFile_ExportsAppContentToFile()
    {
        // Arrange
        var applicationUser = await CreateApplicationUserWithAppContentAsync();
        var tempFile = Path.GetTempFileName();
        var command = GetExportCommand();

        try
        {
            // Act
            var parseResult = command.Parse($"--user-id {applicationUser.UserId} --file {tempFile}");
            var result = await parseResult.InvokeAsync();

            // Assert
            Assert.Equal(0, result);
            Assert.True(File.Exists(tempFile));
            var json = await File.ReadAllTextAsync(tempFile);
            var deserializedAppContent = JsonSerializer.Deserialize<AppContent>(json, JsonOptions);
            Assert.NotNull(deserializedAppContent);
            Assert.Equal(applicationUser.AppContent?.OneLoginCannotFindRecordEmailTemplateId, deserializedAppContent!.OneLoginCannotFindRecordEmailTemplateId);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task Export_WithInvalidUserId_ReturnsError()
    {
        // Arrange
        var invalidUserId = Guid.NewGuid();
        var command = GetExportCommand();

        // Act
        var parseResult = command.Parse($"--user-id {invalidUserId}");
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Import_WithValidJsonFile_ImportsAppContent()
    {
        // Arrange
        var applicationUser = await CreateApplicationUserWithoutAppContentAsync();
        var appContent = new AppContent
        {
            OneLoginCannotFindRecordEmailTemplateId = "template-123",
            OneLoginNotVerifiedEmailTemplateId = "template-456",
            OneLoginRecordMatchedEmailTemplateId = "template-789",
            SupportEmailAddress = "support@test.com"
        };
        var tempFile = Path.GetTempFileName();
        var json = JsonSerializer.Serialize(appContent, JsonOptions);
        await File.WriteAllTextAsync(tempFile, json);

        var command = GetImportCommand();

        try
        {
            // Act
            var parseResult = command.Parse($"--user-id {applicationUser.UserId} --file {tempFile}");
            var result = await parseResult.InvokeAsync();

            // Assert
            Assert.Equal(0, result);

            var updatedUser = await WithDbContextAsync(async dbContext =>
                await dbContext.ApplicationUsers
                    .Where(u => u.UserId == applicationUser.UserId)
                    .SingleAsync());

            Assert.NotNull(updatedUser.AppContent);
            Assert.Equal(appContent.OneLoginCannotFindRecordEmailTemplateId, updatedUser.AppContent!.OneLoginCannotFindRecordEmailTemplateId);
            Assert.Equal(appContent.SupportEmailAddress, updatedUser.AppContent.SupportEmailAddress);

            var processEvent = await WithDbContextAsync(async dbContext =>
                await dbContext.ProcessEvents
                    .Where(e => e.EventName == "ApplicationUserUpdatedEvent")
                    .OrderByDescending(e => e.CreatedOn)
                    .FirstOrDefaultAsync());

            Assert.NotNull(processEvent);
            var eventData = Assert.IsType<ApplicationUserUpdatedEvent>(processEvent!.Payload);
            Assert.Equal(ApplicationUserUpdatedEventChanges.AppContent, eventData.Changes);
            Assert.Equal(applicationUser.UserId, eventData.ApplicationUser.UserId);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task Import_WithExistingAppContent_OverwritesAppContent()
    {
        // Arrange
        var applicationUser = await CreateApplicationUserWithAppContentAsync();
        var originalAppContent = applicationUser.AppContent;

        var newAppContent = new AppContent
        {
            OneLoginCannotFindRecordEmailTemplateId = "new-template-123",
            OneLoginNotVerifiedEmailTemplateId = "new-template-456",
            SupportEmailAddress = "newsupport@example.com"
        };

        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, JsonSerializer.Serialize(newAppContent, JsonOptions));

        var command = GetImportCommand();

        try
        {
            // Act
            var parseResult = command.Parse($"--user-id {applicationUser.UserId} --file {tempFile}");
            var result = await parseResult.InvokeAsync();

            // Assert
            Assert.Equal(0, result);

            var updatedUser = await WithDbContextAsync(async dbContext =>
                await dbContext.ApplicationUsers
                    .Where(u => u.UserId == applicationUser.UserId)
                    .SingleAsync());

            Assert.NotNull(updatedUser.AppContent);
            Assert.Equal(newAppContent.OneLoginCannotFindRecordEmailTemplateId, updatedUser.AppContent!.OneLoginCannotFindRecordEmailTemplateId);
            Assert.Equal(newAppContent.OneLoginNotVerifiedEmailTemplateId, updatedUser.AppContent.OneLoginNotVerifiedEmailTemplateId);
            Assert.Equal(newAppContent.SupportEmailAddress, updatedUser.AppContent.SupportEmailAddress);
            Assert.NotEqual(originalAppContent!.OneLoginCannotFindRecordEmailTemplateId, updatedUser.AppContent.OneLoginCannotFindRecordEmailTemplateId);
            Assert.NotEqual(originalAppContent.SupportEmailAddress, updatedUser.AppContent.SupportEmailAddress);

            var processEvent = await WithDbContextAsync(async dbContext =>
                await dbContext.ProcessEvents
                    .Where(e => e.EventName == "ApplicationUserUpdatedEvent")
                    .OrderByDescending(e => e.CreatedOn)
                    .FirstOrDefaultAsync());

            Assert.NotNull(processEvent);
            var eventData = Assert.IsType<ApplicationUserUpdatedEvent>(processEvent!.Payload);
            Assert.Equal(ApplicationUserUpdatedEventChanges.AppContent, eventData.Changes);
            Assert.Equal(applicationUser.UserId, eventData.ApplicationUser.UserId);
            Assert.NotNull(eventData.OldApplicationUser);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task Import_WithInvalidJson_ReturnsError()
    {
        // Arrange
        var applicationUser = await CreateApplicationUserWithoutAppContentAsync();
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "{ invalid json }");

        var command = GetImportCommand();

        try
        {
            // Act
            var parseResult = command.Parse($"--user-id {applicationUser.UserId} --file {tempFile}");
            var result = await parseResult.InvokeAsync();

            // Assert
            Assert.Equal(1, result);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task Import_WithNonExistentFile_ReturnsError()
    {
        // Arrange
        var applicationUser = await CreateApplicationUserWithoutAppContentAsync();
        var nonExistentFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");

        var command = GetImportCommand();

        // Act
        var parseResult = command.Parse($"--user-id {applicationUser.UserId} --file {nonExistentFile}");
        var result = await parseResult.InvokeAsync();

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task Import_WithInvalidUserId_ReturnsError()
    {
        // Arrange
        var invalidUserId = Guid.NewGuid();
        var appContent = new AppContent { SupportEmailAddress = "test@example.com" };
        var tempFile = Path.GetTempFileName();
        var json = JsonSerializer.Serialize(appContent, JsonOptions);
        await File.WriteAllTextAsync(tempFile, json);

        var command = GetImportCommand();

        try
        {
            // Act
            var parseResult = command.Parse($"--user-id {invalidUserId} --file {tempFile}");
            var result = await parseResult.InvokeAsync();

            // Assert
            Assert.Equal(1, result);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    private Command GetExportCommand() => Commands.CreateAppContentCommand(Configuration).Subcommands.Single(c => c.Name == "export");

    private Command GetImportCommand() => Commands.CreateAppContentCommand(Configuration).Subcommands.Single(c => c.Name == "import");

    private async Task<ApplicationUser> CreateApplicationUserWithAppContentAsync()
    {
        var applicationUser = new ApplicationUser
        {
            UserId = Guid.NewGuid(),
            Name = "Test Application",
            AppContent = new AppContent
            {
                OneLoginCannotFindRecordEmailTemplateId = "template-abc",
                OneLoginNotVerifiedEmailTemplateId = "template-def",
                SupportEmailAddress = "test@example.com"
            }
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.ApplicationUsers.Add(applicationUser);
            await dbContext.SaveChangesAsync();
        });

        return applicationUser;
    }

    private async Task<ApplicationUser> CreateApplicationUserWithoutAppContentAsync()
    {
        var applicationUser = new ApplicationUser
        {
            UserId = Guid.NewGuid(),
            Name = "Test Application Without AppContent",
            AppContent = null
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.ApplicationUsers.Add(applicationUser);
            await dbContext.SaveChangesAsync();
        });

        return applicationUser;
    }
}
