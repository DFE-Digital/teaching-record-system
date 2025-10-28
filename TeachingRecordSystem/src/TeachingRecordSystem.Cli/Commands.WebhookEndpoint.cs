using System.CommandLine.Parsing;
using System.Linq.Expressions;
using System.Text.Json;
using TeachingRecordSystem.Core.ApiSchema;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.Cli;

public partial class Commands
{
    public static Command CreateWebhookEndpointCommand(IConfiguration configuration)
    {
        var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

        Expression<Func<WebhookEndpoint, object>> getEndpointOutputForDisplay = e =>
            new
            {
                e.WebhookEndpointId,
                e.ApplicationUserId,
                ApplicationUserName = e.ApplicationUser!.Name,
                e.ApiVersion,
                e.Address,
                e.CloudEventTypes,
                e.Enabled
            };

        return new Command("webhook-endpoint", "Commands for managing webhook endpoints.")
        {
            CreateCreateCommand(),
            CreateDeleteCommand(),
            CreateGetCommand(),
            CreateListCommand(),
            CreateUpdateCommand()
        };

        static string ParseApiVersionArgument(ArgumentResult result)
        {
            var apiVersion = NormalizeApiVersion(result.Tokens.SingleOrDefault()?.Value ?? "");
            if (!VersionRegistry.AllV3MinorVersions.Contains(apiVersion))
            {
                result.AddError($"'{apiVersion}' is not a valid API version.");
            }
            return apiVersion;
        }

        static string NormalizeApiVersion(string version) => version.StartsWith('V') ? version[1..] : version;

        Command CreateCreateCommand()
        {
            var userIdOption = new Option<Guid>("--user-id") { Required = true };
            var addressOption = new Option<string>("--address") { Required = true };
            var cloudEventTypesOption = new Option<string[]>("--cloud-event-types") { Required = true, AllowMultipleArgumentsPerToken = true };
            var apiVersionOption = new Option<string>("--api-version") { Required = true, CustomParser = ParseApiVersionArgument };
            var enabledOption = new Option<bool>("--enabled") { Required = false };
            var connectionStringOption = new Option<string>("--connection-string") { Required = true };

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
            }

            enabledOption.DefaultValueFactory = _ => true;

            var command = new Command("create", "Creates a webhook endpoint.")
            {
                userIdOption,
                addressOption,
                cloudEventTypesOption,
                apiVersionOption,
                enabledOption,
                connectionStringOption
            };

            command.SetAction(
                async parseResult =>
                {
                    var userId = parseResult.GetRequiredValue(userIdOption);
                    var address = parseResult.GetRequiredValue(addressOption);
                    var cloudEventTypes = parseResult.GetRequiredValue(cloudEventTypesOption);
                    var apiVersion = parseResult.GetRequiredValue(apiVersionOption);
                    var enabled = parseResult.GetValue(enabledOption);
                    var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                    await using var dbContext = TrsDbContext.Create(connectionString);

                    var webhookEndpointId = Guid.NewGuid();
                    var now = DateTime.UtcNow;

                    var endpoint = new WebhookEndpoint()
                    {
                        WebhookEndpointId = webhookEndpointId,
                        ApplicationUserId = userId,
                        Address = address,
                        ApiVersion = apiVersion,
                        CloudEventTypes = cloudEventTypes.Order().ToList(),
                        Enabled = enabled,
                        CreatedOn = now,
                        UpdatedOn = now
                    };

                    dbContext.WebhookEndpoints.Add(endpoint);

                    dbContext.AddEventWithoutBroadcast(new WebhookEndpointCreatedEvent
                    {
                        WebhookEndpoint = EventModels.WebhookEndpoint.FromModel(endpoint),
                        EventId = Guid.NewGuid(),
                        CreatedUtc = now,
                        RaisedBy = SystemUser.SystemUserId
                    });

                    await dbContext.SaveChangesAsync();

                    var printableEndpoint = await dbContext.WebhookEndpoints
                        .Where(e => e.WebhookEndpointId == webhookEndpointId)
                        .OrderBy(e => e.CreatedOn)
                        .Select(getEndpointOutputForDisplay)
                        .SingleAsync();

                    var output = JsonSerializer.Serialize(printableEndpoint, jsonSerializerOptions);
                    Console.WriteLine(output);
                });

            return command;
        }

        Command CreateDeleteCommand()
        {
            var webhookEndpointIdOption = new Option<Guid>("--webhook-endpoint-id", "--id") { Required = true };
            var connectionStringOption = new Option<string>("--connection-string") { Required = true };

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
            }

            var command = new Command("delete", "Deletes a webhook endpoint.")
            {
                webhookEndpointIdOption,
                connectionStringOption
            };

            command.SetAction(
                async parseResult =>
                {
                    var webhookEndpointId = parseResult.GetRequiredValue(webhookEndpointIdOption);
                    var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                    await using var dbContext = TrsDbContext.Create(connectionString);

                    var endpoint = await dbContext.WebhookEndpoints
                        .SingleAsync(e => e.WebhookEndpointId == webhookEndpointId);

                    var now = DateTime.UtcNow;
                    endpoint.DeletedOn = now;

                    dbContext.AddEventWithoutBroadcast(new WebhookEndpointDeletedEvent
                    {
                        WebhookEndpoint = EventModels.WebhookEndpoint.FromModel(endpoint),
                        EventId = Guid.NewGuid(),
                        CreatedUtc = now,
                        RaisedBy = SystemUser.SystemUserId
                    });

                    await dbContext.SaveChangesAsync();
                });

            return command;
        }

        Command CreateGetCommand()
        {
            var webhookEndpointIdOption = new Option<Guid>("--webhook-endpoint-id", "--id") { Required = true };
            var connectionStringOption = new Option<string>("--connection-string") { Required = true };

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
            }

            var command = new Command("get", "Gets a webhook endpoint.")
            {
                webhookEndpointIdOption,
                connectionStringOption
            };

            command.SetAction(
                async parseResult =>
                {
                    var webhookEndpointId = parseResult.GetRequiredValue(webhookEndpointIdOption);
                    var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                    await using var dbContext = TrsDbContext.Create(connectionString);

                    var endpoint = await dbContext.WebhookEndpoints
                        .Include(e => e.ApplicationUser)
                        .SingleAsync(e => e.WebhookEndpointId == webhookEndpointId);

                    var printableEndpoint = new[] { endpoint }.AsQueryable().Select(getEndpointOutputForDisplay).Single();

                    var output = JsonSerializer.Serialize(printableEndpoint, jsonSerializerOptions);
                    Console.WriteLine(output);
                });

            return command;
        }

        Command CreateListCommand()
        {
            var connectionStringOption = new Option<string>("--connection-string") { Required = true };

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
            }

            var command = new Command("list", "Lists the webhook endpoints.")
            {
                connectionStringOption
            };

            command.SetAction(
                async parseResult =>
                {
                    var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                    await using var dbContext = TrsDbContext.Create(connectionString);

                    var endpoints = await dbContext.WebhookEndpoints
                        .Where(e => e.ApplicationUser!.Active)
                        .OrderBy(e => e.CreatedOn)
                        .Select(getEndpointOutputForDisplay)
                        .ToListAsync();

                    var output = JsonSerializer.Serialize(endpoints, jsonSerializerOptions);
                    Console.WriteLine(output);
                });

            return command;
        }

        Command CreateUpdateCommand()
        {
            var webhookEndpointIdOption = new Option<Guid>("--webhook-endpoint-id", "--id") { Required = true };
            var addressOption = new Option<string>("--address") { Required = false };
            var cloudEventTypesOption = new Option<string[]>("--cloud-event-types") { Required = false, AllowMultipleArgumentsPerToken = true };
            var apiVersionOption = new Option<string>("--api-version") { Required = false, CustomParser = ParseApiVersionArgument };
            var enabledOption = new Option<bool>("--enabled") { Required = false };
            var connectionStringOption = new Option<string>("--connection-string") { Required = true };

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
            }

            enabledOption.DefaultValueFactory = _ => true;

            var command = new Command("update", "Updates a webhook endpoint.")
            {
                webhookEndpointIdOption,
                addressOption,
                cloudEventTypesOption,
                apiVersionOption,
                enabledOption,
                connectionStringOption
            };

            command.SetAction(
                async parseResult =>
                {
                    var webhookEndpointId = parseResult.GetRequiredValue(webhookEndpointIdOption);
                    var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                    await using var dbContext = TrsDbContext.Create(connectionString);

                    var endpoint = await dbContext.WebhookEndpoints
                        .Include(e => e.ApplicationUser)
                        .SingleAsync(e => e.WebhookEndpointId == webhookEndpointId);

                    var changes = WebhookEndpointUpdatedChanges.None;

                    if (parseResult.GetValue(addressOption) is { } address)
                    {
                        endpoint.Address = address;
                        changes |= WebhookEndpointUpdatedChanges.Address;
                    }

                    if (parseResult.GetValue(cloudEventTypesOption) is { } cloudEventTypes)
                    {
                        endpoint.CloudEventTypes = cloudEventTypes.Order().ToList();
                        changes |= WebhookEndpointUpdatedChanges.CloudEventTypes;
                    }

                    if (parseResult.GetValue(apiVersionOption) is { } apiVersion)
                    {
                        endpoint.ApiVersion = apiVersion;
                        changes |= WebhookEndpointUpdatedChanges.ApiVersion;
                    }

                    if (parseResult.GetValue(enabledOption) is { } enabled)
                    {
                        endpoint.Enabled = enabled;
                        changes |= WebhookEndpointUpdatedChanges.Enabled;
                    }

                    if (changes != WebhookEndpointUpdatedChanges.None)
                    {
                        var now = DateTime.UtcNow;
                        endpoint.UpdatedOn = now;

                        dbContext.AddEventWithoutBroadcast(new WebhookEndpointUpdatedEvent
                        {
                            EventId = Guid.NewGuid(),
                            CreatedUtc = now,
                            RaisedBy = SystemUser.SystemUserId,
                            WebhookEndpoint = EventModels.WebhookEndpoint.FromModel(endpoint),
                            Changes = changes
                        });

                        await dbContext.SaveChangesAsync();
                    }

                    var printableEndpoint = new[] { endpoint }.AsQueryable().Select(getEndpointOutputForDisplay).Single();

                    var output = JsonSerializer.Serialize(printableEndpoint, jsonSerializerOptions);
                    Console.WriteLine(output);
                });

            return command;
        }
    }
}
