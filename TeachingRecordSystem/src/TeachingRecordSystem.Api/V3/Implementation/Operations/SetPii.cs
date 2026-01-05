using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record SetPiiCommand : ICommand<SetPiiResult>
{
    public required string Trn { get; init; }
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
}

public record SetPiiResult;

public class SetPiiHandler(
    TrsDbContext dbContext,
    ICurrentUserProvider currentUserProvider,
    IClock clock,
    PersonService personService) :
    ICommandHandler<SetPiiCommand, SetPiiResult>
{
    public async Task<ApiResult<SetPiiResult>> ExecuteAsync(SetPiiCommand command)
    {
        var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.Trn == command.Trn);

        if (person is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        var currentUserId = currentUserProvider.GetCurrentApplicationUserId();
        if (!person.AllowDetailsUpdatesFromSourceApplication ||
            person.SourceApplicationUserId != currentUserId)
        {
            return ApiError.PiiUpdatesForbidden();
        }

        if (person.QtsDate is not null)
        {
            return ApiError.PiiUpdatesForbiddenPersonHasQts();
        }

        if (person.EytsDate is not null)
        {
            return ApiError.PiiUpdatesForbiddenPersonHasEyts();
        }

        var now = clock.UtcNow;

        var processContext = new ProcessContext(ProcessType.PersonDetailsUpdating, now, currentUserId);

        await personService.UpdatePersonDetailsAsync(new(
            person.PersonId,
            new PersonDetails()
            {
                FirstName = command.FirstName,
                MiddleName = command.MiddleName ?? string.Empty,
                LastName = command.LastName,
                DateOfBirth = command.DateOfBirth,
                EmailAddress = command.EmailAddress is string emailAddress ? EmailAddress.Parse(emailAddress) : null,
                NationalInsuranceNumber = command.NationalInsuranceNumber is string nino ? NationalInsuranceNumber.Parse(nino) : null,
                Gender = command.Gender,
            }.UpdateAll(),
            null,
            null),
            processContext);

        return new SetPiiResult();
    }
}
