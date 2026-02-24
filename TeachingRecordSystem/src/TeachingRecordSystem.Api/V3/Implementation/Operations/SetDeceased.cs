using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record SetDeceasedCommand(string Trn, DateOnly DateOfDeath) : ICommand<SetDeceasedResult>;

public record SetDeceasedResult;

public class SetDeceasedHandler(
    PersonService personService,
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

        var currentUserId = currentUserProvider.GetCurrentApplicationUserId();

        var processContext = new ProcessContext(ProcessType.PersonDeceased, clock.UtcNow, currentUserId);

        await personService.DeactivatePersonAsync(
            new DeactivatePersonOptions(person.PersonId, command.DateOfDeath),
            processContext);

        return new SetDeceasedResult();
    }
}
