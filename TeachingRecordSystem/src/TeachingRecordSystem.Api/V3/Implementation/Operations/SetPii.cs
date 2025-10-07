using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.Legacy;

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
    IClock clock) :
    ICommandHandler<SetPiiCommand, SetPiiResult>
{
    public async Task<ApiResult<SetPiiResult>> ExecuteAsync(SetPiiCommand command)
    {
        var person = await dbContext.Persons.SingleOrDefaultAsync(p => p.Trn == command.Trn);

        if (person is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        var (currentUserId, _) = currentUserProvider.GetCurrentApplicationUser();
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

        var updateResult = person.UpdateDetails(
            command.FirstName,
            command.MiddleName ?? string.Empty,
            command.LastName,
            command.DateOfBirth,
            command.EmailAddress is string emailAddress ? EmailAddress.Parse(emailAddress) : null,
            command.NationalInsuranceNumber is string nino ? NationalInsuranceNumber.Parse(nino) : null,
            command.Gender,
            now);

        var personUpdatedEvent = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = currentUserId,
            PersonId = person.PersonId,
            PersonAttributes = updateResult.PersonAttributes,
            OldPersonAttributes = updateResult.OldPersonAttributes,
            NameChangeReason = null,
            NameChangeEvidenceFile = null,
            DetailsChangeReason = null,
            DetailsChangeReasonDetail = null,
            DetailsChangeEvidenceFile = null,
            Changes = PersonDetailsUpdatedEventChanges.None
        };
        await dbContext.AddEventAndBroadcastAsync(personUpdatedEvent);

        await dbContext.SaveChangesAsync();

        return new SetPiiResult();
    }
}
