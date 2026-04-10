using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.GetAnIdentity;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.Cli;

public partial class Commands
{
    public static Command MarkDeceasedCommand(IConfiguration configuration)
    {
        var trn = new Option<string>("--trn") { Required = true };
        var dateOfDeath = new Option<DateOnly>("--date-of-death") { Required = true };
        var connectionStringOption = new Option<string>("--connection-string") { Required = true };
        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (configuredConnectionString is not null)
        {
            connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
        }

        var command =
            new Command("mark-deceased",
                "Marks a person as deceased")
            {
                trn, connectionStringOption,dateOfDeath
            };

        command.SetAction(async parseResult =>
        {
            var connectionString = parseResult.GetRequiredValue(connectionStringOption);
            var environment = new HostingEnvironment { EnvironmentName = Environments.Production };

            var services = new ServiceCollection()
                .AddClock()
                .AddDatabase(connectionString)
                .AddTrnRequestService(configuration)
                .AddPersonService()
                .AddOneLoginService()
                .AddSupportTaskService()
                .AddWebhookOptions(configuration)
                .AddWebhookDeliveryService(configuration)
                .AddWebhookMessageFactory()
                .AddMemoryCache()
                .AddIdentityApi(configuration)
                .AddEventPublisher()
                .AddBackgroundJobScheduler(environment)
                .AddHangfire(environment);

            services.AddDbContext<IdDbContext>(
                options => options.UseInMemoryDatabase("TeacherAuthId"),
                contextLifetime: ServiceLifetime.Transient);

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var personService = scope.ServiceProvider.GetRequiredService<PersonService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<TrsDbContext>();
            var processContext = new ProcessContext(processType: ProcessType.PersonDeceased, now: DateTime.UtcNow,
                SystemUser.SystemUserId);
            var person = await dbContext.Persons.SingleAsync(x => x.Trn == parseResult.GetRequiredValue(trn));
            await personService.DeactivatePersonAsync(
                new DeactivatePersonOptions(
                    PersonId: person.PersonId,
                    DateOfDeath: parseResult.GetRequiredValue(dateOfDeath)),
                processContext);
        });

        return command;
    }
}
