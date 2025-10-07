using System.CommandLine.Invocation;
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

        var command = new Command("webhook-endpoint", "Commands for managing webhook endpoints.");
        command.AddCommand(CreateCreateCommand());
        command.AddCommand(CreateDeleteCommand());
        command.AddCommand(CreateGetCommand());
        command.AddCommand(CreateListCommand());
        command.AddCommand(CreateUpdateCommand());
        return command;

        static string ParseApiVersionArgument(ArgumentResult result)
        {
            var apiVersion = NormalizeApiVersion(result.Tokens.SingleOrDefault()?.Value ?? "");
            if (!VersionRegistry.AllV3MinorVersions.Contains(apiVersion))
            {
                result.ErrorMessage = $"'{apiVersion}' is not a valid API version.";
            }
            return apiVersion;
        }

        static string NormalizeApiVersion(string version) => version.StartsWith('V') ? version[1..] : version;

        Command CreateCreateCommand()
        {
            var userIdOption = new Option<Guid>("--user-id") { IsRequired = true };
            var addressOption = new Option<string>("--address") { IsRequired = true };
            var cloudEventTypesOption = new Option<string[]>("--cloud-event-types") { IsRequired = true, AllowMultipleArgumentsPerToken = true };
            var apiVersionOption = new Option<string>("--api-version", ParseApiVersionArgument) { IsRequired = true };
            var enabledOption = new Option<bool>("--enabled") { IsRequired = false };
            var connectionStringOption = new Option<string>("--connection-string") { IsRequired = true };

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.SetDefaultValue(configuredConnectionString);
            }

            enabledOption.SetDefaultValue(true);

            var command = new Command("create", "Creates a webhook endpoint.")
            {
                userIdOption,
                addressOption,
                cloudEventTypesOption,
                apiVersionOption,
                enabledOption,
                connectionStringOption
            };

            command.SetHandler(
                async (Guid userId, string address, string[] cloudEventTypes, string apiVersion, bool enable, string connectionString) =>
                {
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
                        Enabled = enable,
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
                },
                userIdOption,
                addressOption,
                cloudEventTypesOption,
                apiVersionOption,
                enabledOption,
                connectionStringOption);

            return command;
        }

        Command CreateDeleteCommand()
        {
            var webhookEndpointIdOption = new Option<Guid>(["--webhook-endpoint-id", "--id"]) { IsRequired = true };
            var connectionStringOption = new Option<string>("--connection-string") { IsRequired = true };

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.SetDefaultValue(configuredConnectionString);
            }

            var command = new Command("delete", "Deletes a webhook endpoint.")
            {
                webhookEndpointIdOption,
                connectionStringOption
            };

            command.SetHandler(
                async (Guid webhookEndpointId, string connectionString) =>
                {
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
                },
                webhookEndpointIdOption,
                connectionStringOption);

            return command;
        }

        Command CreateGetCommand()
        {
            var webhookEndpointIdOption = new Option<Guid>(["--webhook-endpoint-id", "--id"]) { IsRequired = true };
            var connectionStringOption = new Option<string>("--connection-string") { IsRequired = true };

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.SetDefaultValue(configuredConnectionString);
            }

            var command = new Command("get", "Gets a webhook endpoint.")
            {
                webhookEndpointIdOption,
                connectionStringOption
            };

            command.SetHandler(
                async (Guid webhookEndpointId, string connectionString) =>
                {
                    await using var dbContext = TrsDbContext.Create(connectionString);

                    var endpoint = await dbContext.WebhookEndpoints
                        .Include(e => e.ApplicationUser)
                        .SingleAsync(e => e.WebhookEndpointId == webhookEndpointId);

                    var printableEndpoint = new[] { endpoint }.AsQueryable().Select(getEndpointOutputForDisplay).Single();

                    var output = JsonSerializer.Serialize(printableEndpoint, jsonSerializerOptions);
                    Console.WriteLine(output);
                },
                webhookEndpointIdOption,
                connectionStringOption);

            return command;
        }

        Command CreateListCommand()
        {
            var connectionStringOption = new Option<string>("--connection-string") { IsRequired = true };

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.SetDefaultValue(configuredConnectionString);
            }

            var command = new Command("list", "Lists the webhook endpoints.")
            {
                connectionStringOption
            };

            command.SetHandler(
                async (string connectionString) =>
                {
                    await using var dbContext = TrsDbContext.Create(connectionString);

                    var endpoints = await dbContext.WebhookEndpoints
                        .Where(e => e.ApplicationUser!.Active)
                        .OrderBy(e => e.CreatedOn)
                        .Select(getEndpointOutputForDisplay)
                        .ToListAsync();

                    var output = JsonSerializer.Serialize(endpoints, jsonSerializerOptions);
                    Console.WriteLine(output);
                },
                connectionStringOption);

            return command;
        }

        Command CreateUpdateCommand()
        {
            var webhookEndpointIdOption = new Option<Guid>(["--webhook-endpoint-id", "--id"]) { IsRequired = true };
            var addressOption = new Option<string>("--address") { IsRequired = false };
            var cloudEventTypesOption = new Option<string[]>("--cloud-event-types") { IsRequired = false, AllowMultipleArgumentsPerToken = true };
            var apiVersionOption = new Option<string>("--api-version", ParseApiVersionArgument) { IsRequired = false };
            var enabledOption = new Option<bool>("--enabled") { IsRequired = false };
            var connectionStringOption = new Option<string>("--connection-string") { IsRequired = true };

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.SetDefaultValue(configuredConnectionString);
            }

            enabledOption.SetDefaultValue(true);

            var command = new Command("update", "Updates a webhook endpoint.")
            {
                webhookEndpointIdOption,
                addressOption,
                cloudEventTypesOption,
                apiVersionOption,
                enabledOption,
                connectionStringOption
            };

            command.SetHandler(
                async (InvocationContext context) =>
                {
                    var webhookEndpointId = context.ParseResult.GetValueForOption(webhookEndpointIdOption);
                    var connectionString = context.ParseResult.GetValueForOption(connectionStringOption)!;

                    await using var dbContext = TrsDbContext.Create(connectionString);

                    var endpoint = await dbContext.WebhookEndpoints
                        .Include(e => e.ApplicationUser)
                        .SingleAsync(e => e.WebhookEndpointId == webhookEndpointId);

                    var changes = WebhookEndpointUpdatedChanges.None;

                    if (context.ParseResult.HasOption(addressOption))
                    {
                        endpoint.Address = context.ParseResult.GetValueForOption(addressOption)!;
                        changes |= WebhookEndpointUpdatedChanges.Address;
                    }

                    if (context.ParseResult.HasOption(cloudEventTypesOption))
                    {
                        endpoint.CloudEventTypes = context.ParseResult.GetValueForOption(cloudEventTypesOption)!.Order().ToList();
                        changes |= WebhookEndpointUpdatedChanges.CloudEventTypes;
                    }

                    if (context.ParseResult.HasOption(apiVersionOption))
                    {
                        endpoint.ApiVersion = context.ParseResult.GetValueForOption(apiVersionOption)!;
                        changes |= WebhookEndpointUpdatedChanges.ApiVersion;
                    }

                    if (context.ParseResult.HasOption(enabledOption))
                    {
                        endpoint.Enabled = context.ParseResult.GetValueForOption(enabledOption);
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
