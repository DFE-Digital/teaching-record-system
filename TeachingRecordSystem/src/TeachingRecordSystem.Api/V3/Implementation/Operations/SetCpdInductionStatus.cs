using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record SetCpdInductionStatusCommand(
    string Trn,
    InductionStatus Status,
    DateOnly? StartDate,
    DateOnly? CompletedDate,
    DateTime CpdModifiedOn) :
    ICommand<SetCpdInductionStatusResult>;

public record SetCpdInductionStatusResult;

public class SetCpdInductionStatusHandler(TrsDbContext dbContext, ICurrentUserProvider currentUserProvider, IClock clock) :
    ICommandHandler<SetCpdInductionStatusCommand, SetCpdInductionStatusResult>
{
    public async Task<ApiResult<SetCpdInductionStatusResult>> ExecuteAsync(SetCpdInductionStatusCommand command)
    {
        if (command.Status is not InductionStatus.RequiredToComplete and not InductionStatus.InProgress
            and not InductionStatus.Passed and not InductionStatus.Failed)
        {
            return ApiError.InvalidInductionStatus(command.Status);
        }

        if (command.Status.RequiresStartDate() && command.StartDate is null)
        {
            return ApiError.InductionStartDateIsRequired(command.Status);
        }

        if (!command.Status.RequiresStartDate() && command.StartDate is not null)
        {
            return ApiError.InductionStartDateIsNotPermitted(command.Status);
        }

        if (command.Status.RequiresCompletedDate() && command.CompletedDate is null)
        {
            return ApiError.InductionCompletedDateIsRequired(command.Status);
        }

        if (!command.Status.RequiresCompletedDate() && command.CompletedDate is not null)
        {
            return ApiError.InductionCompletedDateIsNotPermitted(command.Status);
        }

        var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.Trn == command.Trn);

        if (person is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        if (person.QtsDate is null)
        {
            return ApiError.PersonDoesNotHaveQts(command.Trn);
        }

        if (person.CpdInductionCpdModifiedOn is DateTime cpdInductionCpdModifiedOn &&
            command.CpdModifiedOn < cpdInductionCpdModifiedOn)
        {
            return ApiError.StaleRequest(cpdInductionCpdModifiedOn);
        }

        var currentUserId = currentUserProvider.GetCurrentApplicationUserId();

        person.SetCpdInductionStatus(
            command.Status,
            command.StartDate,
            command.CompletedDate,
            command.CpdModifiedOn,
            currentUserId,
            clock.UtcNow,
            out var updatedEvent);

        if (updatedEvent is not null)
        {
            await dbContext.AddEventAndBroadcastAsync(updatedEvent);
        }

        await dbContext.SaveChangesAsync();

        return new SetCpdInductionStatusResult();
    }
}
