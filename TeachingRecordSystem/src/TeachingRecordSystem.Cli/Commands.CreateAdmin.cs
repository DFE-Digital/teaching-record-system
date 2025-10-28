using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Cli;

public static partial class Commands
{
    public static Command CreateCreateAdminCommand(IConfiguration configuration)
    {
        var emailOption = new Option<string>("--email") { Required = true };
        var nameOption = new Option<string>("--name") { Required = true };
        var connectionStringOption = new Option<string>("--connection-string") { Required = true };

        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (configuredConnectionString is not null)
        {
            connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
        }

        var command = new Command("create-admin", $"Creates a new user with the {UserRoles.Administrator} role.")
        {
            emailOption,
            nameOption,
            connectionStringOption
        };

        command.Validators.Add(commandResult =>
        {
            var email = commandResult.GetRequiredValue(emailOption);

            if (!email.EndsWith("@education.gov.uk", StringComparison.OrdinalIgnoreCase))
            {
                commandResult.AddError("Email address must be an @education.gov.uk address.");
            }
        });

        command.SetAction(
            async parseResult =>
            {
                var email = parseResult.GetRequiredValue(emailOption);
                var name = parseResult.GetRequiredValue(nameOption);
                var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                using var dbContext = TrsDbContext.Create(connectionString);

                dbContext.Users.Add(new()
                {
                    Active = true,
                    Email = email,
                    Name = name,
                    Role = UserRoles.Administrator,
                    UserId = Guid.NewGuid()
                });

                await dbContext.SaveChangesAsync();
            });

        return command;
    }
}
