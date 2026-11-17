using System.CommandLine.Parsing;
using System.Linq.Expressions;
using System.Text.Json;
using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.ApiSchema;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.Cli;

public partial class Commands
{
    private static readonly TimeSpan _pingPollInterval = TimeSpan.FromMilliseconds(500);

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
            CreateUpdateCommand(),
            CreatePingCommand()
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

        static ServiceProvider CreateServices(string connectionString) =>
            new ServiceCollection()
                .AddTimeProvider()
                .AddDatabase(connectionString)
                .AddMemoryCache()
                .AddWebhookMessageFactory()
                .AddEventPublisher()
                .BuildServiceProvider();

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

                    await using var services = CreateServices(connectionString);

                    using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                    using var scope = services.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<TrsDbContext>();
                    var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
                    var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

                    var now = timeProvider.UtcNow;

                    var webhookEndpointId = Guid.NewGuid();

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
                    await dbContext.SaveChangesAsync();

                    var processContext = new ProcessContext(ProcessType.WebhookEndpointCreating, now, SystemUser.SystemUserId);

                    await eventPublisher.PublishSingleEventAsync(
                        new WebhookEndpointCreatedEvent
                        {
                            EventId = Guid.NewGuid(),
                            WebhookEndpoint = EventModels.WebhookEndpoint.FromModel(endpoint)
                        },
                        processContext);

                    var printableEndpoint = await dbContext.WebhookEndpoints
                        .Where(e => e.WebhookEndpointId == webhookEndpointId)
                        .OrderBy(e => e.CreatedOn)
                        .Select(getEndpointOutputForDisplay)
                        .SingleAsync();

                    transaction.Complete();

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

                    await using var services = CreateServices(connectionString);

                    using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                    using var scope = services.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<TrsDbContext>();
                    var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
                    var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

                    var now = timeProvider.UtcNow;

                    var endpoint = await dbContext.WebhookEndpoints
                        .SingleAsync(e => e.WebhookEndpointId == webhookEndpointId);

                    endpoint.DeletedOn = now;
                    await dbContext.SaveChangesAsync();

                    var processContext = new ProcessContext(ProcessType.WebhookEndpointDeleting, now, SystemUser.SystemUserId);

                    await eventPublisher.PublishSingleEventAsync(
                        new WebhookEndpointDeletedEvent
                        {
                            EventId = Guid.NewGuid(),
                            WebhookEndpoint = EventModels.WebhookEndpoint.FromModel(endpoint)
                        },
                        processContext);

                    transaction.Complete();
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

                    await using var services = CreateServices(connectionString);

                    using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                    using var scope = services.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<TrsDbContext>();
                    var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
                    var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

                    var endpoint = await dbContext.WebhookEndpoints
                        .Include(e => e.ApplicationUser)
                        .SingleAsync(e => e.WebhookEndpointId == webhookEndpointId);

                    var changes = WebhookEndpointUpdatedEventChanges.None;

                    if (parseResult.GetValue(addressOption) is { } address)
                    {
                        endpoint.Address = address;
                        changes |= WebhookEndpointUpdatedEventChanges.Address;
                    }

                    if (parseResult.GetResult(cloudEventTypesOption)?.Implicit is false &&
                        parseResult.GetValue(cloudEventTypesOption) is { } cloudEventTypes)
                    {
                        endpoint.CloudEventTypes = cloudEventTypes.Order().ToList();
                        changes |= WebhookEndpointUpdatedEventChanges.CloudEventTypes;
                    }

                    if (parseResult.GetValue(apiVersionOption) is { } apiVersion)
                    {
                        endpoint.ApiVersion = apiVersion;
                        changes |= WebhookEndpointUpdatedEventChanges.ApiVersion;
                    }

                    if (parseResult.GetValue(enabledOption) is { } enabled)
                    {
                        endpoint.Enabled = enabled;
                        changes |= WebhookEndpointUpdatedEventChanges.Enabled;
                    }

                    if (changes != WebhookEndpointUpdatedEventChanges.None)
                    {
                        var now = timeProvider.UtcNow;
                        endpoint.UpdatedOn = now;
                        await dbContext.SaveChangesAsync();

                        var processContext = new ProcessContext(ProcessType.WebhookEndpointUpdating, now, SystemUser.SystemUserId);

                        await eventPublisher.PublishSingleEventAsync(
                            new WebhookEndpointUpdatedEvent
                            {
                                EventId = Guid.NewGuid(),
                                WebhookEndpoint = EventModels.WebhookEndpoint.FromModel(endpoint),
                                Changes = changes
                            },
                            processContext);
                    }

                    var printableEndpoint = new[] { endpoint }.AsQueryable().Select(getEndpointOutputForDisplay).Single();

                    transaction.Complete();

                    var output = JsonSerializer.Serialize(printableEndpoint, jsonSerializerOptions);
                    Console.WriteLine(output);
                });

            return command;
        }

        Command CreatePingCommand()
        {
            var webhookEndpointIdOption = new Option<Guid>("--webhook-endpoint-id", "--id") { Required = true };
            var noWaitOption = new Option<bool>("--no-wait")
            {
                Required = false,
                Description = "Exit as soon as the ping message is queued instead of waiting for the first delivery attempt."
            };
            var timeoutOption = new Option<int>("--timeout")
            {
                Required = false,
                Description = "The maximum number of seconds to wait for the first delivery attempt.",
                DefaultValueFactory = _ => 120
            };
            var connectionStringOption = new Option<string>("--connection-string") { Required = true };

            timeoutOption.Validators.Add(result =>
            {
                if (result.GetValueOrDefault<int>() <= 0)
                {
                    result.AddError("--timeout must be greater than zero.");
                }
            });

            var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
            if (configuredConnectionString is not null)
            {
                connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
            }

            var command = new Command(
                "ping",
                "Queues a ping message to be sent to a webhook endpoint, allowing integrators to test their signature verification without triggering a 'real' event. The message is delivered by the webhook delivery background service; by default this command waits for the first delivery attempt and reports its outcome.")
            {
                webhookEndpointIdOption,
                noWaitOption,
                timeoutOption,
                connectionStringOption
            };

            command.SetAction(
                async (parseResult, cancellationToken) =>
                {
                    var webhookEndpointId = parseResult.GetRequiredValue(webhookEndpointIdOption);
                    var noWait = parseResult.GetValue(noWaitOption);
                    var timeout = TimeSpan.FromSeconds(parseResult.GetValue(timeoutOption));
                    var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                    await using var services = new ServiceCollection()
                        .AddTimeProvider()
                        .AddDatabase(connectionString)
                        .AddMemoryCache()
                        .AddWebhookMessageFactory()
                        .BuildServiceProvider();

                    using var scope = services.CreateScope();
                    var webhookMessageFactory = scope.ServiceProvider.GetRequiredService<WebhookMessageFactory>();

                    var message = await webhookMessageFactory.CreatePingMessageAsync(webhookEndpointId);
                    var endpoint = message.WebhookEndpoint!;

                    Console.WriteLine($"Ping message '{message.WebhookMessageId}' was queued for delivery to endpoint '{webhookEndpointId}'.");

                    if (!endpoint.Enabled)
                    {
                        Console.Error.WriteLine("Endpoint is disabled; the message will not be delivered until it is enabled.");
                        return noWait ? 0 : 1;
                    }

                    if (noWait)
                    {
                        return 0;
                    }

                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(timeout);

                    var dbContext = scope.ServiceProvider.GetRequiredService<TrsDbContext>();
                    WebhookMessage? attemptedMessage = null;

                    await using (ConsoleSpinner.Start($"Waiting for first delivery attempt to {endpoint.Address}"))
                    {
                        try
                        {
                            while (true)
                            {
                                var current = await dbContext.WebhookMessages
                                    .AsNoTracking()
                                    .SingleAsync(m => m.WebhookMessageId == message.WebhookMessageId, timeoutCts.Token);

                                if (current.DeliveryAttempts.Count > 0)
                                {
                                    attemptedMessage = current;
                                    break;
                                }

                                await Task.Delay(_pingPollInterval, timeoutCts.Token);
                            }
                        }
                        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                        {
                        }
                    }

                    if (attemptedMessage is null)
                    {
                        Console.Error.WriteLine(
                            $"✗ Timed out after {timeout.TotalSeconds:N0}s waiting for a delivery attempt. Is the webhook delivery service running? " +
                            "The message remains queued and may still be delivered.");
                        return 1;
                    }

                    if (attemptedMessage.Delivered is not null)
                    {
                        Console.WriteLine($"✓ Ping delivered successfully to {endpoint.Address}.");
                        return 0;
                    }

                    Console.Error.WriteLine($"✗ Delivery to {endpoint.Address} failed: {attemptedMessage.DeliveryErrors.LastOrDefault() ?? "Unknown error"}");

                    if (attemptedMessage.NextDeliveryAttempt is { } nextDeliveryAttempt)
                    {
                        Console.Error.WriteLine($"  The next delivery attempt is scheduled for {nextDeliveryAttempt:u}.");
                    }

                    return 1;
                });

            return command;
        }
    }
}
