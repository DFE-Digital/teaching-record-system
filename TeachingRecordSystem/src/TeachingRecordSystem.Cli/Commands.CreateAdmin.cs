using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Cli;

public static partial class Commands
{
    public static Command CreateCreateAdminCommand(IConfiguration configuration)
    {
        var emailOption = new Option<string>("--email") { IsRequired = true };
        var nameOption = new Option<string>("--name") { IsRequired = true };
        var connectionStringOption = new Option<string>("--connection-string") { IsRequired = true };

        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (configuredConnectionString is not null)
        {
            connectionStringOption.SetDefaultValue(configuredConnectionString);
        }

        var command = new Command("create-admin", $"Creates a new user with the {UserRoles.Administrator} role.")
        {
            emailOption,
            nameOption,
            connectionStringOption
        };

        command.AddValidator(commandResult =>
        {
            var email = commandResult.GetValueForOption(emailOption);

            if (email?.EndsWith("@education.gov.uk", StringComparison.OrdinalIgnoreCase) != true)
            {
                commandResult.ErrorMessage = "Email address must be an @education.gov.uk address.";
            }
        });

        command.SetHandler(
            async (string email, string name, string connectionString) =>
            {
                using var dbContext = TrsDbContext.Create(connectionString);

                dbContext.Users.Add(new()
                {
                    Active = true,
                    Email = email,
                    Name = name,
                    Roles = [UserRoles.Administrator],
                    UserId = Guid.NewGuid()
                });

                await dbContext.SaveChangesAsync();
            },
            emailOption,
            nameOption,
            connectionStringOption);

        return command;
    }
}
