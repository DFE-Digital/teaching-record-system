using Microsoft.Xrm.Sdk.Query;
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
    public required string? EmailAddresses { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
}

public class SetPiiHandler(TrsDbContext dbContext, ICrmQueryDispatcher crmQueryDispatcher)
{
    public async Task<ApiResult<Unit>> HandleAsync(SetPiiCommand command)
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
                EmailAddress: command.EmailAddresses));

        return Unit.Instance;
    }
}
