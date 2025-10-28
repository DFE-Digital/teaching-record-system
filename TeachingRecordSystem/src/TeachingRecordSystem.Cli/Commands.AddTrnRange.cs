using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Cli;

public partial class Commands
{
    public static Command CreateAddTrnRangeCommand(IConfiguration configuration)
    {
        var fromOption = new Option<int>("--from") { Required = true };
        var toOption = new Option<int>("--to") { Required = true };
        var nextOption = new Option<int?>("--next") { Required = false };
        var connectionStringOption = new Option<string>("--connection-string") { Required = true };

        var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (configuredConnectionString is not null)
        {
            connectionStringOption.DefaultValueFactory = _ => configuredConnectionString;
        }

        var command = new Command("add-trn-range", "Adds a new TRN range.")
        {
            fromOption,
            toOption,
            nextOption,
            connectionStringOption
        };

        command.Validators.Add(commandResult =>
        {
            var to = commandResult.GetRequiredValue(toOption);
            var from = commandResult.GetRequiredValue(fromOption);
            var next = commandResult.GetValue(nextOption);

            if (from is < 1000000 or > 9999999)
            {
                commandResult.AddError("--from must be between 1000000 and 9999999.");
                return;
            }

            if (to is < 1000000 or > 9999999)
            {
                commandResult.AddError("--to must be between 1000000 and 9999999.");
                return;
            }

            if (to <= from)
            {
                commandResult.AddError("--to must be greater than --from.");
                return;
            }

            if (next < from || next > to)
            {
                commandResult.AddError("--next must be between --from and --to.");
                return;
            }
        });

        command.SetAction(
            async parseResult =>
            {
                var from = parseResult.GetRequiredValue(fromOption);
                var to = parseResult.GetRequiredValue(toOption);
                var next = parseResult.GetValue(nextOption);
                var connectionString = parseResult.GetRequiredValue(connectionStringOption);

                using var dbContext = TrsDbContext.Create(connectionString);

                dbContext.TrnRanges.Add(new TrnRange
                {
                    FromTrn = from,
                    ToTrn = to,
                    NextTrn = next ?? from,
                    IsExhausted = false
                });

                await dbContext.SaveChangesAsync();
            });

        return command;
    }
}
