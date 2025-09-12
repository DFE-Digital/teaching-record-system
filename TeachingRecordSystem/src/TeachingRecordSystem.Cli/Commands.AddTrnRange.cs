using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Cli;

public partial class Commands
{
    public static Command CreateAddTrnRangeCommand(IConfiguration configuration)
    {
        var fromOption = new Option<int>("--from") { IsRequired = true };
        var toOption = new Option<int>("--to") { IsRequired = true };
        var nextOption = new Option<int?>("--next") { IsRequired = false };
        var connectionStringOption = new Option<string>("--connection-string") { IsRequired = true };

        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (configuredConnectionString is not null)
        {
            connectionStringOption.SetDefaultValue(configuredConnectionString);
        }

        var command = new Command("add-trn-range", "Adds a new TRN range.")
        {
            fromOption,
            toOption,
            nextOption,
            connectionStringOption
        };

        command.AddValidator(commandResult =>
        {
            var to = commandResult.GetValueForOption(toOption);
            var from = commandResult.GetValueForOption(fromOption);
            var next = commandResult.GetValueForOption(nextOption);

            if (from is < 1000000 or > 9999999)
            {
                commandResult.ErrorMessage = "--from must be between 1000000 and 9999999.";
                return;
            }

            if (to is < 1000000 or > 9999999)
            {
                commandResult.ErrorMessage = "--to must be between 1000000 and 9999999.";
                return;
            }

            if (to <= from)
            {
                commandResult.ErrorMessage = "--to must be greater than --from.";
                return;
            }

            if (next < from || next > to)
            {
                commandResult.ErrorMessage = "--next must be between --from and --to.";
                return;
            }
        });

        command.SetHandler(
            async (int from, int to, int? next, string connectionString) =>
            {
                using var dbContext = TrsDbContext.Create(connectionString);

                dbContext.TrnRanges.Add(new TrnRange
                {
                    FromTrn = from,
                    ToTrn = to,
                    NextTrn = next ?? from,
                    IsExhausted = false
                });

                await dbContext.SaveChangesAsync();
            },
            fromOption,
            toOption,
            nextOption,
            connectionStringOption);

        return command;
    }
}
