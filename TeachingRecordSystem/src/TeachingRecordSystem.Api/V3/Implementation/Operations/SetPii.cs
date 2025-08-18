using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record SetPiiCommand
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

public class SetPiiHandler(
    TrsDbContext dbContext,
    ICurrentUserProvider currentUserProvider,
    IClock clock,
    ICrmQueryDispatcher crmQueryDispatcher,
    IFeatureProvider featureProvider)
{
    public async Task<ApiResult<Unit>> HandleAsync(SetPiiCommand command)
    {
        if (!featureProvider.IsEnabled(FeatureNames.ContactsMigrated))
        {
            return await HandleOverDqtAsync(command);
        }

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

        return Unit.Instance;
    }

    private async Task<ApiResult<Unit>> HandleOverDqtAsync(SetPiiCommand command)
    {
        var contact = await crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactByTrnQuery(
                command.Trn,
                new ColumnSet(
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_AllowPiiUpdatesFromRegister,
                    Contact.Fields.dfeta_EYTSDate)));

        if (contact is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        // Normalize names; DQT matching process requires a single-word first name :-|
        var firstAndMiddleNames = $"{command.FirstName} {command.MiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var firstName = firstAndMiddleNames.First();
        var middleName = string.Join(' ', firstAndMiddleNames.Skip(1));

        // return an error if the contact does not permit updates from register
        if (contact.dfeta_AllowPiiUpdatesFromRegister == false)
        {
            return ApiError.PiiUpdatesForbidden();
        }

        var person = await dbContext.Persons.SingleAsync(p => p.PersonId == contact.Id);

        // return error if contact has qts.
        if (person.QtsDate.HasValue)
        {
            return ApiError.PiiUpdatesForbiddenPersonHasQts();
        }

        if (person.EytsDate.HasValue)
        {
            return ApiError.PiiUpdatesForbiddenPersonHasEyts();
        }

        await crmQueryDispatcher.ExecuteQueryAsync(
            new UpdateContactPiiQuery(
                ContactId: contact.Id,
                FirstName: firstName,
                MiddleName: middleName,
                LastName: command.LastName,
                DateOfBirth: command.DateOfBirth,
                NationalInsuranceNumber: NationalInsuranceNumber.Normalize(command.NationalInsuranceNumber),
                Gender: command.Gender?.ToContact_GenderCode(),
                EmailAddress: command.EmailAddress));

        return Unit.Instance;
    }
}
