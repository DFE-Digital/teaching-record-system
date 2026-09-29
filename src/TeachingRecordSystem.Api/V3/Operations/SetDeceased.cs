using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.Api.V3.Operations;

public record SetDeceasedCommand(string Trn, DateOnly DateOfDeath) : ICommand<SetDeceasedResult>;

public record SetDeceasedResult;

public class SetDeceasedHandler(
    PersonService personService,
    TrsDbContext dbContext,
    ICurrentUserProvider currentUserProvider,
    TimeProvider timeProvider) :
    ICommandHandler<SetDeceasedCommand, SetDeceasedResult>
{
    public async Task<ApiResult<SetDeceasedResult>> ExecuteAsync(SetDeceasedCommand command, CancellationToken cancellationToken)
    {
        var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.Trn == command.Trn, cancellationToken);

        if (person is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        var currentUserId = currentUserProvider.GetCurrentApplicationUserId();

        var processContext = new ProcessContext(ProcessType.PersonDeceased, timeProvider.UtcNow, currentUserId);

        await personService.DeactivatePersonAsync(
            new DeactivatePersonOptions(person.PersonId, command.DateOfDeath),
            processContext,
            cancellationToken);

        return new SetDeceasedResult();
    }
}
