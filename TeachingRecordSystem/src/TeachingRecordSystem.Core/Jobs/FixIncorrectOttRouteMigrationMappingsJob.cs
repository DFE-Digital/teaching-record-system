using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Jobs;

public class FixIncorrectOttRouteMigrationMappingsJob(
    IDbContextFactory<TrsDbContext> dbContextFactory,
    ReferenceDataCache referenceDataCache,
    IClock clock)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var allRoutes = await referenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Database.SetCommandTimeout(0);

        var professionalStatusToFix = dbContext
            .RouteToProfessionalStatuses
            .Include(r => r.Person)
            .Where(r => r.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.OverseasTrainedTeacherProgrammeId && r.DqtTeacherStatusValue == "103")
            .ToListAsync();

        foreach (var professionalStatus in await professionalStatusToFix)
        {
            professionalStatus.Update(
                allRoutes,
                r =>
                {
                    r.RouteToProfessionalStatusTypeId = RouteToProfessionalStatusType.OverseasTrainedTeacherRecognitionId;
                },
                changeReason: "AnotherReason",
                changeReasonDetail: "Route type incorrectly mapped during migration",
                evidenceFile: null,
                updatedBy: SystemUser.SystemUserId,
                clock.UtcNow,
                out var updatedEvent);

            if (updatedEvent is not null)
            {
                await dbContext.AddEventAndBroadcastAsync(updatedEvent);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
