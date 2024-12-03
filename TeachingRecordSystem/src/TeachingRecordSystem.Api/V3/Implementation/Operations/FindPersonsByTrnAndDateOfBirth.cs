using System.Collections.Immutable;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record FindPersonsByTrnAndDateOfBirthCommand(IEnumerable<(string Trn, DateOnly DateOfBirth)> Persons);

public record FindPersonsByTrnAndDateOfBirthResult(int Total, IReadOnlyCollection<FindPersonsByTrnAndDateOfBirthResultItem> Items);

public record FindPersonsByTrnAndDateOfBirthResultItem
{
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required IReadOnlyCollection<SanctionInfo> Sanctions { get; init; }
    public required IReadOnlyCollection<Alert> Alerts { get; init; }
    public required IReadOnlyCollection<NameInfo> PreviousNames { get; init; }
    public required InductionStatusInfo? InductionStatus { get; init; }
    public required QtsInfo? Qts { get; init; }
    public required EytsInfo? Eyts { get; init; }
}

public class FindPersonsByTrnAndDateOfBirthHandler(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    PreviousNameHelper previousNameHelper,
    ReferenceDataCache referenceDataCache)
{
    public async Task<ApiResult<FindPersonsByTrnAndDateOfBirthResult>> HandleAsync(FindPersonsByTrnAndDateOfBirthCommand command)
    {
        var contacts = await crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactsByTrnsQuery(
                command.Persons.Select(p => p.Trn).Where(trn => !string.IsNullOrEmpty(trn)).Distinct(),
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.BirthDate,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.dfeta_StatedFirstName,
                    Contact.Fields.dfeta_StatedMiddleName,
                    Contact.Fields.dfeta_StatedLastName,
                    Contact.Fields.dfeta_InductionStatus)));

        // Remove any results where the request DOB doesn't match the contact's DOB
        // (we can't easily do this in the query itself).
        var matched = contacts
            .Where(kvp => kvp.Value is not null)
            .Where(kvp => command.Persons.First(p => p.Trn == kvp.Key).DateOfBirth == kvp.Value!.BirthDate?.ToDateOnlyWithDqtBstFix(isLocalTime: false))
            .Select(kvp => kvp.Value!)
            .ToArray();

        var contactsById = matched.ToDictionary(c => c.Id, c => c);

        var getAlertsTask = dbContext.Alerts
            .Include(a => a.AlertType)
            .ThenInclude(at => at.AlertCategory)
            .Where(a => contactsById.Keys.ToArray().Contains(a.PersonId))
            .GroupBy(a => a.PersonId)
            .ToDictionaryAsync(a => a.Key, a => a.ToArray());

        var getPreviousNamesTask = crmQueryDispatcher.ExecuteQueryAsync(new GetPreviousNamesByContactIdsQuery(contactsById.Keys));

        var getQtsRegistrationsTask = crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveQtsRegistrationsByContactIdsQuery(
                contactsById.Keys,
                new ColumnSet(
                    dfeta_qtsregistration.Fields.CreatedOn,
                    dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId,
                    dfeta_qtsregistration.Fields.dfeta_EYTSDate,
                    dfeta_qtsregistration.Fields.dfeta_QTSDate,
                    dfeta_qtsregistration.Fields.dfeta_PersonId,
                    dfeta_qtsregistration.Fields.dfeta_TeacherStatusId)));

        await Task.WhenAll(getAlertsTask, getPreviousNamesTask, getQtsRegistrationsTask);

        var alerts = getAlertsTask.Result;
        var previousNames = getPreviousNamesTask.Result;
        var qtsRegistrations = getQtsRegistrationsTask.Result;

        return new FindPersonsByTrnAndDateOfBirthResult(
            Total: matched.Length,
            Items: await matched
                .ToAsyncEnumerable()
                .SelectAwait(async r => new FindPersonsByTrnAndDateOfBirthResultItem()
                {
                    Trn = r.dfeta_TRN,
                    DateOfBirth = r.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false),
                    FirstName = r.ResolveFirstName(),
                    MiddleName = r.ResolveMiddleName(),
                    LastName = r.ResolveLastName(),
                    Sanctions = alerts.GetValueOrDefault(r.Id, [])
                        .Where(a => Constants.LegacyExposableSanctionCodes.Contains(a.AlertType.DqtSanctionCode) && a.IsOpen)
                        .Select(a => new SanctionInfo()
                        {
                            Code = a.AlertType.DqtSanctionCode!,
                            StartDate = a.StartDate
                        })
                        .AsReadOnly(),
                    Alerts = alerts.GetValueOrDefault(r.Id, [])
                        .Where(a => !a.AlertType.InternalOnly)
                        .Select(a =>
                        {
                            return new Alert()
                            {
                                AlertId = a.AlertId,
                                AlertType = new()
                                {
                                    AlertTypeId = a.AlertType.AlertTypeId,
                                    AlertCategory = new()
                                    {
                                        AlertCategoryId = a.AlertType.AlertCategory.AlertCategoryId,
                                        Name = a.AlertType.AlertCategory.Name
                                    },
                                    Name = a.AlertType.Name,
                                    DqtSanctionCode = a.AlertType.DqtSanctionCode!
                                },
                                Details = a.Details,
                                StartDate = a.StartDate,
                                EndDate = a.EndDate
                            };
                        })
                        .AsReadOnly(),
                    PreviousNames = previousNameHelper.GetFullPreviousNames(previousNames[r.Id], contactsById[r.Id])
                        .Select(name => new NameInfo()
                        {
                            FirstName = name.FirstName,
                            MiddleName = name.MiddleName,
                            LastName = name.LastName
                        })
                        .AsReadOnly(),
                    InductionStatus = r.dfeta_InductionStatus?.ConvertToInductionStatus() is Dtos.InductionStatus inductionStatus ?
                        new InductionStatusInfo()
                        {
                            Status = inductionStatus,
                            StatusDescription = inductionStatus.GetDescription()
                        } :
                        null,
                    Qts = await QtsInfo.CreateAsync(qtsRegistrations[r.Id].OrderBy(qr => qr.CreatedOn).FirstOrDefault(s => s.dfeta_QTSDate is not null), referenceDataCache),
                    Eyts = await EytsInfo.CreateAsync(qtsRegistrations[r.Id].OrderBy(qr => qr.CreatedOn).FirstOrDefault(s => s.dfeta_EYTSDate is not null), referenceDataCache),
                })
                .OrderBy(c => c.Trn)
                .ToArrayAsync());
    }
}
