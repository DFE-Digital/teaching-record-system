using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record SetWelshInductionStatusCommand(string Trn, bool Passed, DateOnly StartDate, DateOnly CompletedDate) : ICommand<SetWelshInductionStatusResult>;

public record SetWelshInductionStatusResult;

public class SetWelshInductionStatusHandler(
    TrsDbContext dbContext,
    ICurrentUserProvider currentUserProvider,
    IClock clock) :
    ICommandHandler<SetWelshInductionStatusCommand, SetWelshInductionStatusResult>
{
    public async Task<ApiResult<SetWelshInductionStatusResult>> ExecuteAsync(SetWelshInductionStatusCommand command)
    {
        var person = await dbContext.Persons
            .Include(p => p.Qualifications)
            .SingleOrDefaultAsync(p => p.Trn == command.Trn);

        if (person is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        if (person.QtsDate is null)
        {
            return ApiError.PersonDoesNotHaveQts(command.Trn);
        }

        var (currentUserId, _) = currentUserProvider.GetCurrentApplicationUser();

        person.TrySetWelshInductionStatus(
            command.Passed,
            !command.Passed ? command.StartDate : null,
            !command.Passed ? command.CompletedDate : null,
            currentUserId,
            clock.UtcNow,
            out var updatedEvent);

        if (updatedEvent is not null)
        {
            await dbContext.AddEventAndBroadcastAsync(updatedEvent);
        }

        await dbContext.SaveChangesAsync();

        return new SetWelshInductionStatusResult();
    }
}
