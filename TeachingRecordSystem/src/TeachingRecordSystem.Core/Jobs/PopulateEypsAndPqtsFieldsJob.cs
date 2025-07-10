using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.Core.Jobs;

public class PopulateEypsAndPqtsFieldsJob(
    IDbContextFactory<TrsDbContext> dbContextFactory,
    ReferenceDataCache referenceDataCache,
    IClock clock)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var allRouteTypes = await referenceDataCache.GetRouteToProfessionalStatusTypesAsync();

        var readDbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        readDbContext.Database.SetCommandTimeout(0);

        var personIds = readDbContext.Database.SqlQuery<QueryResult>(
                $"""
                select distinct q.person_id from qualifications q
                join route_to_professional_status_types r on q.route_to_professional_status_type_id = r.route_to_professional_status_type_id
                where r.professional_status_type in (2, 3) and q.holds_from is not null
                """)
            .AsAsyncEnumerable();

        await foreach (var r in personIds.WithCancellation(cancellationToken))
        {
            await using var writeDbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var person = await writeDbContext.Persons
                .IgnoreQueryFilters()
                .Include(p => p.Qualifications)
                .SingleAsync(p => p.PersonId == r.person_id);

            var eypsRoute = person.Qualifications!
                .OfType<RouteToProfessionalStatus>()
                .Where(r => r.Status == RouteToProfessionalStatusStatus.Holds && r.RouteToProfessionalStatusType!.ProfessionalStatusType == ProfessionalStatusType.EarlyYearsProfessionalStatus)
                .OrderBy(r => r.CreatedOn)
                .FirstOrDefault();

            if (eypsRoute is not null)
            {
                eypsRoute.Update(
                    allRouteTypes,
                    _ => { },
                    changeReason: null,
                    changeReasonDetail: null,
                    evidenceFile: null,
                    updatedBy: SystemUser.SystemUserId,
                    now: clock.UtcNow,
                    out var @event);

                if (@event is not null)
                {
                    writeDbContext.AddEventWithoutBroadcast(@event);
                    await writeDbContext.SaveChangesAsync(cancellationToken);
                }
            }

            var pqtsRoute = person.Qualifications!
                .OfType<RouteToProfessionalStatus>()
                .Where(r => r.Status == RouteToProfessionalStatusStatus.Holds && r.RouteToProfessionalStatusType!.ProfessionalStatusType == ProfessionalStatusType.PartialQualifiedTeacherStatus)
                .OrderBy(r => r.HoldsFrom)
                .FirstOrDefault();

            if (pqtsRoute is not null)
            {
                pqtsRoute.Update(
                    allRouteTypes,
                    _ => { },
                    changeReason: null,
                    changeReasonDetail: null,
                    evidenceFile: null,
                    updatedBy: SystemUser.SystemUserId,
                    now: clock.UtcNow,
                    out var @event);

                if (@event is not null)
                {
                    writeDbContext.AddEventWithoutBroadcast(@event);
                    await writeDbContext.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }

    private record QueryResult(Guid person_id);
}
