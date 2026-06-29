using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.Cli;

public partial class Commands
{
    public static Command CreateAppContentCommand(IConfiguration configuration)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return new Command("app-content", "Commands for managing application user content.")
        {
            CreateExportCommand(),
            CreateImportCommand()
        };

        Command CreateExportCommand()
        {
            var userIdOption = new Option<Guid>("--user-id") { Required = true };
            var fileOption = new Option<string?>("--file") { Required = false };
            var connectionStringOption = new Option<string>("--connection-string") { Required = true };

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
            }

            var command = new Command("export", "Exports the AppContent for an application user as JSON.")
            {
                userIdOption,
                fileOption,
                connectionStringOption
            };

            command.SetAction(
                async parseResult =>
                {
                    var userId = parseResult.GetRequiredValue(userIdOption);
                    var file = parseResult.GetValue(fileOption);
                    var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                    using var dbContext = TrsDbContext.Create(connectionString);

                    var applicationUser = await dbContext.ApplicationUsers
                        .Where(u => u.UserId == userId)
                        .SingleOrDefaultAsync();

                    if (applicationUser is null)
                    {
                        Console.Error.WriteLine($"Application user with ID '{userId}' not found.");
                        return 1;
                    }

                    var json = JsonSerializer.Serialize(applicationUser.AppContent, jsonSerializerOptions);

                    if (file is not null)
                    {
                        await File.WriteAllTextAsync(file, json);
                        Console.WriteLine($"AppContent exported to {file}");
                    }
                    else
                    {
                        Console.WriteLine(json);
                    }

                    return 0;
                });

            return command;
        }

        Command CreateImportCommand()
        {
            var userIdOption = new Option<Guid>("--user-id") { Required = true };
            var fileOption = new Option<string?>("--file") { Required = false };
            var connectionStringOption = new Option<string>("--connection-string") { Required = true };

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
            }

            var command = new Command("import", "Imports AppContent for an application user from JSON.")
            {
                userIdOption,
                fileOption,
                connectionStringOption
            };

            command.SetAction(
                async parseResult =>
                {
                    var userId = parseResult.GetRequiredValue(userIdOption);
                    var file = parseResult.GetValue(fileOption);
                    var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                    string json;
                    if (file is not null)
                    {
                        if (!File.Exists(file))
                        {
                            Console.Error.WriteLine($"File '{file}' not found.");
                            return 1;
                        }

                        json = await File.ReadAllTextAsync(file);
                    }
                    else
                    {
                        json = await Console.In.ReadToEndAsync();
                    }

                    AppContent? appContent;
                    try
                    {
                        appContent = JsonSerializer.Deserialize<AppContent>(json, jsonSerializerOptions);
                    }
                    catch (JsonException ex)
                    {
                        Console.Error.WriteLine($"Invalid JSON: {ex.Message}");
                        return 1;
                    }

                    var services = new ServiceCollection()
                        .AddTimeProvider()
                        .AddDatabase(connectionString)
                        .AddMemoryCache()
                        .AddWebhookMessageFactory()
                        .AddEventPublisher()
                        .BuildServiceProvider();

                    using var scope = services.CreateScope();
                    var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<TrsDbContext>();

                    var applicationUser = await dbContext.ApplicationUsers
                        .Where(u => u.UserId == userId)
                        .SingleOrDefaultAsync();

                    if (applicationUser is null)
                    {
                        Console.Error.WriteLine($"Application user with ID '{userId}' not found.");
                        return 1;
                    }

                    var oldApplicationUser = EventModels.ApplicationUser.FromModel(applicationUser);

                    applicationUser.AppContent = appContent;

                    var @event = new ApplicationUserUpdatedEvent()
                    {
                        EventId = Guid.NewGuid(),
                        CreatedUtc = timeProvider.UtcNow,
                        RaisedBy = SystemUser.SystemUserId,
                        ApplicationUser = EventModels.ApplicationUser.FromModel(applicationUser),
                        OldApplicationUser = oldApplicationUser,
                        Changes = ApplicationUserUpdatedEventChanges.AppContent
                    };

                    dbContext.AddEventWithoutBroadcast(@event);

                    await dbContext.SaveChangesAsync();

                    // Notify TeacherAuth about changes to the application user
                    await dbContext.Database.ExecuteSqlRawAsync($"NOTIFY {ChannelNames.OneLoginClient}");

                    Console.WriteLine($"AppContent imported successfully for user {userId}");

                    return 0;
                });

            return command;
        }
    }
}
