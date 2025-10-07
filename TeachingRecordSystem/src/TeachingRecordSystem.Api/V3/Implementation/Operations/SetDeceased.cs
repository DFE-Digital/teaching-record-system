using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record SetDeceasedCommand(string Trn, DateOnly DateOfDeath) : ICommand<SetDeceasedResult>;

public record SetDeceasedResult;

public class SetDeceasedHandler(
    TrsDbContext dbContext,
    ICurrentUserProvider currentUserProvider,
    IClock clock) :
    ICommandHandler<SetDeceasedCommand, SetDeceasedResult>
{
    public async Task<ApiResult<SetDeceasedResult>> ExecuteAsync(SetDeceasedCommand command)
    {
        var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.Trn == command.Trn);

        if (person is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        var (currentUserId, _) = currentUserProvider.GetCurrentApplicationUser();

        person.Status = PersonStatus.Deactivated;
        person.DateOfDeath = command.DateOfDeath;

        var @event = new PersonStatusUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = clock.UtcNow,
            RaisedBy = currentUserId,
            PersonId = person.PersonId,
            Status = PersonStatus.Deactivated,
            OldStatus = PersonStatus.Active,
            Reason = "Deceased",
            ReasonDetail = null,
            EvidenceFile = null,
            DateOfDeath = command.DateOfDeath
        };
        await dbContext.AddEventAndBroadcastAsync(@event);

        await dbContext.SaveChangesAsync();

        return new SetDeceasedResult();
    }
}
