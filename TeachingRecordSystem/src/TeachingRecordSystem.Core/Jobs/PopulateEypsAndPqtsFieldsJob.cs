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

        var writeDbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var persons = readDbContext.Persons.Include(p => p.Qualifications).AsNoTracking();

        await foreach (var person in persons.ToAsyncEnumerable().WithCancellation(cancellationToken))
        {
            var eypsRoute = person.Qualifications!
                .OfType<RouteToProfessionalStatus>()
                .Where(r => r.Status == RouteToProfessionalStatusStatus.Holds && r.RouteToProfessionalStatusType!.ProfessionalStatusType == ProfessionalStatusType.EarlyYearsProfessionalStatus)
                .OrderBy(r => r.CreatedOn)
                .FirstOrDefault();

            if (eypsRoute is not null)
            {
                writeDbContext.Attach(person);

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
                writeDbContext.Attach(person);

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
}
