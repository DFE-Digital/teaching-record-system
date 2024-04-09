using System.Collections.Immutable;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record FindTeachersCommand(string LastName, DateOnly? DateOfBirth);

public record FindTeachersResult(int Total, IReadOnlyCollection<FindTeachersResultItem> Items);

public record FindTeachersResultItem
{
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required IReadOnlyCollection<SanctionInfo> Sanctions { get; init; }
    public required IReadOnlyCollection<NameInfo> PreviousNames { get; init; }
}

public class FindTeachersHandler(ICrmQueryDispatcher crmQueryDispatcher, IConfiguration configuration)
{
    private readonly TimeSpan _concurrentNameChangeWindow = TimeSpan.FromSeconds(configuration.GetValue("ConcurrentNameChangeWindowSeconds", 5));

    public async Task<FindTeachersResult> Handle(FindTeachersCommand command)
    {
        var contacts = await crmQueryDispatcher.ExecuteQuery(
            new GetActiveContactsByLastNameAndDateOfBirthQuery(
                command.LastName!,
                command.DateOfBirth!.Value,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.BirthDate,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.dfeta_StatedFirstName,
                    Contact.Fields.dfeta_StatedMiddleName,
                    Contact.Fields.dfeta_StatedLastName)));

        var contactsById = contacts.ToDictionary(r => r.Id, r => r);

        var sanctions = await crmQueryDispatcher.ExecuteQuery(
            new GetSanctionsByContactIdsQuery(
                contactsById.Keys,
                ActiveOnly: true,
                new()));

        var previousNames = (await crmQueryDispatcher.ExecuteQuery(new GetPreviousNamesByContactIdsQuery(contactsById.Keys)))
            .ToDictionary(
                kvp => kvp.Key,
                kvp => PreviousNameHelper.GetFullPreviousNames(kvp.Value, contactsById[kvp.Key], _concurrentNameChangeWindow));

        return new FindTeachersResult(
            Total: contacts.Length,
            Items: contacts.Select(r => new FindTeachersResultItem()
            {
                Trn = r.dfeta_TRN,
                DateOfBirth = r.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false),
                FirstName = r.ResolveFirstName(),
                MiddleName = r.ResolveMiddleName(),
                LastName = r.ResolveLastName(),
                Sanctions = sanctions[r.Id]
                    .Where(s => Constants.ExposableSanctionCodes.Contains(s.SanctionCode))
                    .Select(s => new SanctionInfo()
                    {
                        Code = s.SanctionCode,
                        StartDate = s.Sanction.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true)
                    })
                    .AsReadOnly(),
                PreviousNames = previousNames[r.Id]
                    .Select(name => new NameInfo()
                    {
                        FirstName = name.FirstName,
                        MiddleName = name.MiddleName,
                        LastName = name.LastName
                    })
                    .AsReadOnly()
            })
            .OrderBy(c => c.Trn)
            .AsReadOnly());
    }
}
