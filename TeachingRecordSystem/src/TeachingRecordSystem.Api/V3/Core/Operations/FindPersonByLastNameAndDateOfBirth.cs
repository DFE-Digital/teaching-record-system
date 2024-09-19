using System.Collections.Immutable;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record FindPersonByLastNameAndDateOfBirthCommand(string LastName, DateOnly? DateOfBirth);

public record FindPersonByLastNameAndDateOfBirthResult(int Total, IReadOnlyCollection<FindPersonByLastNameAndDateOfBirthResultItem> Items);

public record FindPersonByLastNameAndDateOfBirthResultItem
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

public class FindPersonByLastNameAndDateOfBirthHandler(
    ICrmQueryDispatcher crmQueryDispatcher,
    PreviousNameHelper previousNameHelper,
    ReferenceDataCache referenceDataCache)
{
    public async Task<FindPersonByLastNameAndDateOfBirthResult> Handle(FindPersonByLastNameAndDateOfBirthCommand command)
    {
        var matched = await crmQueryDispatcher.ExecuteQuery(
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
                    Contact.Fields.dfeta_StatedLastName,
                    Contact.Fields.dfeta_InductionStatus)));

        var contactsById = matched.ToDictionary(r => r.Id, r => r);

        var getSanctionsTask = crmQueryDispatcher.ExecuteQuery(
            new GetSanctionsByContactIdsQuery(
                contactsById.Keys,
                ActiveOnly: true,
                new(dfeta_sanction.Fields.dfeta_StartDate, dfeta_sanction.Fields.dfeta_EndDate)));

        var getPreviousNamesTask = crmQueryDispatcher.ExecuteQuery(new GetPreviousNamesByContactIdsQuery(contactsById.Keys));

        var getQtsRegistrationsTask = crmQueryDispatcher.ExecuteQuery(
            new GetActiveQtsRegistrationsByContactIdsQuery(
                contactsById.Keys,
                new ColumnSet(
                    dfeta_qtsregistration.Fields.CreatedOn,
                    dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId,
                    dfeta_qtsregistration.Fields.dfeta_EYTSDate,
                    dfeta_qtsregistration.Fields.dfeta_QTSDate,
                    dfeta_qtsregistration.Fields.dfeta_PersonId,
                    dfeta_qtsregistration.Fields.dfeta_TeacherStatusId)));

        await Task.WhenAll(getSanctionsTask, getPreviousNamesTask, getQtsRegistrationsTask);

        var sanctions = getSanctionsTask.Result;
        var previousNames = getPreviousNamesTask.Result;
        var qtsRegistrations = getQtsRegistrationsTask.Result;

        return new FindPersonByLastNameAndDateOfBirthResult(
            Total: matched.Length,
            Items: await matched
                .ToAsyncEnumerable()
                .SelectAwait(async r => new FindPersonByLastNameAndDateOfBirthResultItem()
                {
                    Trn = r.dfeta_TRN,
                    DateOfBirth = r.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false),
                    FirstName = r.ResolveFirstName(),
                    MiddleName = r.ResolveMiddleName(),
                    LastName = r.ResolveLastName(),
                    Sanctions = sanctions[r.Id]
                        .Where(s => Constants.LegacyExposableSanctionCodes.Contains(s.SanctionCode))
                        .Select(s => new SanctionInfo()
                        {
                            Code = s.SanctionCode,
                            StartDate = s.Sanction.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true)
                        })
                        .AsReadOnly(),
                    Alerts = await sanctions[r.Id]
                        .ToAsyncEnumerable()
                        .WhereAwait(async s =>
                        {
                            var alertType = await referenceDataCache.GetAlertTypeByDqtSanctionCodeIfExists(s.SanctionCode);

                            if (alertType is null)
                            {
                                return false;
                            }

                            return !alertType.InternalOnly;
                        })
                        .SelectAwait(async s =>
                        {
                            var alertType = await referenceDataCache.GetAlertTypeByDqtSanctionCode(s.SanctionCode);
                            var alertCategory = await referenceDataCache.GetAlertCategoryById(alertType.AlertCategoryId);

                            return new Alert()
                            {
                                AlertId = s.Sanction.Id,
                                AlertType = new()
                                {
                                    AlertTypeId = alertType.AlertTypeId,
                                    AlertCategory = new()
                                    {
                                        AlertCategoryId = alertCategory.AlertCategoryId,
                                        Name = alertCategory.Name
                                    },
                                    Name = alertType.Name,
                                    DqtSanctionCode = alertType.DqtSanctionCode!
                                },
                                StartDate = s.Sanction.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                EndDate = s.Sanction.dfeta_EndDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true)
                            };
                        })
                        .AsReadOnlyAsync(),
                    PreviousNames = previousNameHelper.GetFullPreviousNames(previousNames[r.Id], contactsById[r.Id])
                        .Select(name => new NameInfo()
                        {
                            FirstName = name.FirstName,
                            MiddleName = name.MiddleName,
                            LastName = name.LastName
                        })
                        .AsReadOnly(),
                    InductionStatus = r.dfeta_InductionStatus?.ConvertToInductionStatus() is InductionStatus inductionStatus ?
                        new InductionStatusInfo()
                        {
                            Status = inductionStatus,
                            StatusDescription = inductionStatus.GetDescription()
                        } :
                        null,
                    Qts = await QtsInfo.Create(qtsRegistrations[r.Id].OrderBy(qr => qr.CreatedOn).FirstOrDefault(s => s.dfeta_QTSDate is not null), referenceDataCache),
                    Eyts = await EytsInfo.Create(qtsRegistrations[r.Id].OrderBy(qr => qr.CreatedOn).FirstOrDefault(s => s.dfeta_EYTSDate is not null), referenceDataCache),
                })
                .OrderBy(c => c.Trn)
                .ToArrayAsync());
    }
}
