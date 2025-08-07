using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record FindPersonsResult(int Total, IReadOnlyCollection<FindPersonsResultItem> Items);

public record FindPersonsResultItem
{
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required IReadOnlyCollection<SanctionInfo> Sanctions { get; init; }
    public required IReadOnlyCollection<Alert> Alerts { get; init; }
    public required IReadOnlyCollection<NameInfo> PreviousNames { get; init; }
    public required InductionInfo Induction { get; init; }
    public required DqtInductionStatusInfo? DqtInductionStatus { get; init; }
    public required QtsInfo? Qts { get; init; }
    public required EytsInfo? Eyts { get; init; }
    public required QtlsStatus QtlsStatus { get; init; }
}

public abstract class FindPersonsHandlerBase(
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache)
{
    protected TrsDbContext DbContext => dbContext;

    protected static ColumnSet ContactColumnSet { get; } = new(
        Contact.Fields.dfeta_TRN,
        Contact.Fields.BirthDate,
        Contact.Fields.FirstName,
        Contact.Fields.MiddleName,
        Contact.Fields.LastName,
        Contact.Fields.dfeta_StatedFirstName,
        Contact.Fields.dfeta_StatedMiddleName,
        Contact.Fields.dfeta_StatedLastName,
        Contact.Fields.dfeta_InductionStatus,
        Contact.Fields.dfeta_qtlsdate,
        Contact.Fields.dfeta_QtlsDateHasBeenSet);

    protected async Task<FindPersonsResult> CreateResultAsync(IReadOnlyCollection<Guid> matchedPersonIds)
    {
        var persons = await dbContext.Persons
            .Include(p => p.Alerts!).AsSplitQuery()
            .Include(p => p.Qualifications!).AsSplitQuery()
            .Include(p => p.PreviousNames).AsSplitQuery()
            .Where(p => matchedPersonIds.Contains(p.PersonId))
            .ToDictionaryAsync(p => p.PersonId, p => p);

        var items = await matchedPersonIds
            .ToAsyncEnumerable()
            .Select(id => persons[id])
            .SelectAwait(async person => new FindPersonsResultItem()
            {
                Trn = person.Trn!,
                DateOfBirth = person.DateOfBirth!.Value,
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                LastName = person.LastName,
                Sanctions = person.Alerts!
                    .Where(a => Constants.LegacyExposableSanctionCodes.Contains(a.AlertType!.DqtSanctionCode) && a.IsOpen)
                    .Select(a => new SanctionInfo()
                    {
                        Code = a.AlertType!.DqtSanctionCode!,
                        StartDate = a.StartDate
                    })
                    .AsReadOnly(),
                Alerts = person.Alerts!
                    .Where(a => !a.AlertType!.InternalOnly)
                    .Select(a => new Alert()
                    {
                        AlertId = a.AlertId,
                        AlertType = new()
                        {
                            AlertTypeId = a.AlertType!.AlertTypeId,
                            AlertCategory = new()
                            {
                                AlertCategoryId = a.AlertType.AlertCategory!.AlertCategoryId,
                                Name = a.AlertType.AlertCategory.Name
                            },
                            Name = a.AlertType.Name,
                            DqtSanctionCode = a.AlertType.DqtSanctionCode!
                        },
                        Details = a.Details,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate
                    })
                    .AsReadOnly(),
                PreviousNames = person.PreviousNames!
                    .OrderByDescending(pn => pn.CreatedOn)
                    .Select(name => new NameInfo()
                    {
                        FirstName = name.FirstName,
                        MiddleName = name.MiddleName,
                        LastName = name.LastName
                    })
                    .AsReadOnly(),
                Induction = await InductionInfo.CreateAsync(person, referenceDataCache),
                DqtInductionStatus = person.InductionStatus.ToDqtInductionStatus(out var statusDescription) is string inductionStatus ?
                    new DqtInductionStatusInfo()
                    {
                        Status = Enum.Parse<DqtInductionStatus>(inductionStatus, ignoreCase: true),
                        StatusDescription = statusDescription!
                    } :
                    null,
                Qts = QtsInfo.Create(person),
                Eyts = EytsInfo.Create(person),
                QtlsStatus = person.QtlsStatus
            })
            .OrderBy(c => c.Trn)
            .ToArrayAsync();

        return new FindPersonsResult(items.Length, items);
    }
}
