using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Inductions;

namespace TeachingRecordSystem.Api.V3.Operations;

public record SetWelshInductionStatusCommand(string Trn, bool Passed, DateOnly StartDate, DateOnly CompletedDate) : ICommand<SetWelshInductionStatusResult>;

public record SetWelshInductionStatusResult;

public class SetWelshInductionStatusHandler(
    TrsDbContext dbContext,
    ICurrentUserProvider currentUserProvider,
    TimeProvider timeProvider,
    InductionService inductionService) :
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

        var currentUserId = currentUserProvider.GetCurrentApplicationUserId();

        await inductionService.TrySetWelshInductionStatusAsync(
            new SetWelshInductionStatusOptions
            {
                PersonId = person.PersonId,
                Passed = command.Passed,
                StartDate = !command.Passed ? command.StartDate : null,
                CompletedDate = !command.Passed ? command.CompletedDate : null
            },
            new ProcessContext(ProcessType.PersonWelshInductionUpdating, timeProvider.UtcNow, currentUserId));

        return new SetWelshInductionStatusResult();
    }
}
