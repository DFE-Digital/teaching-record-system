using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Jobs;

public class TempUpdateSetHoldsDateJob(TrsDbContext context,
    ReferenceDataCache referenceDataCache,
    IClock clock)
{
    private static readonly DateOnly QtlsCutoff = new(2012, 4, 1);

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var qtlsRouteId = DataStore.Postgres.Models.RouteToProfessionalStatusType.QtlsAndSetMembershipId;
        var qtlsQualifications = context.Qualifications!.Include(p => p.Person).OfType<DataStore.Postgres.Models.RouteToProfessionalStatus>()
            .Where(p => p.RouteToProfessionalStatusTypeId == qtlsRouteId && p.HoldsFrom < QtlsCutoff)
            .ToArray();

        foreach (var existingQualification in qtlsQualifications)
        {
            existingQualification.Update(
                allRouteTypes: await referenceDataCache.GetRouteToProfessionalStatusTypesAsync(
                    activeOnly: false),
                ps => ps.HoldsFrom = QtlsCutoff,
                changeReason: null,
                changeReasonDetail: "System updated QTLS/SET holds to earliest permissible date when before 01/04/2012",
                evidenceFile: null,
                updatedBy: SystemUser.SystemUserId,
                now: clock.UtcNow,
                out var @event);

            if (@event is not null)
            {
                await context.AddEventAndBroadcastAsync(@event);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
