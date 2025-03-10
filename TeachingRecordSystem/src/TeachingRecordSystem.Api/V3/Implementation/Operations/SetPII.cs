using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record SetPIICommand
{
    public required string Trn { get; init; }
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required IReadOnlyCollection<string> EmailAddresses { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
}

public class SetPIIHandler(ICrmQueryDispatcher crmQueryDispatcher)
{
    public async Task<ApiResult<Unit>> HandleAsync(SetPIICommand command)
    {
        var contact = await crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactByTrnQuery(
                command.Trn,
                new ColumnSet(
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_AllowPiiUpdatesFromRegister)));

        if (contact is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        // Normalize names; DQT matching process requires a single-word first name :-|
        var firstAndMiddleNames = $"{command.FirstName} {command.MiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var firstName = firstAndMiddleNames.First();
        var middleName = string.Join(' ', firstAndMiddleNames.Skip(1));

        //var firstNameSynonyms = (await nameSynonymProvider.GetAllNameSynonyms()).GetValueOrDefault(firstName, []);
        var firstNameSynonyms = Array.Empty<string>();  // Disabled temporarily
        var normalizedNino = NationalInsuranceNumberHelper.Normalize(command.NationalInsuranceNumber);

        // return an error if the contact does not permit updates from register
        if(contact.dfeta_AllowPiiUpdatesFromRegister == false)
        {
            return ApiError.PIIUpdatesForbidden();
        }

        //await crmQueryDispatcher.ExecuteQueryAsync(new UpdateContactPIIQuery()
        //{
        //    ContactId = contact.Id,
        //    FirstName = firstName,
        //    MiddleName = middleName,
        //    LastName = command.LastName,
        //    DateOfBirth = command.DateOfBirth
        //});


        return Unit.Instance;
    }
}
