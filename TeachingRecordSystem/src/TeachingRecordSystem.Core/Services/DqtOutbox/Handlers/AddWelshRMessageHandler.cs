using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

namespace TeachingRecordSystem.Core.Services.DqtOutbox.Handlers;

public class AddWelshRMessageHandler(TrsDbContext dbContext, IClock clock) : IMessageHandler<AddWelshRMessage>
{
    public async Task HandleMessageAsync(AddWelshRMessage message)
    {
        var person = await dbContext.Persons.Include(i => i.Qualifications).SingleAsync(p => p.PersonId == message.PersonId);
        var allRoutes = await dbContext.RoutesToProfessionalStatus.ToArrayAsync();
        var welshrRoute = allRoutes.Single(x => x.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.WelshRId);

        dbContext.RouteToProfessionalStatuses.Add(RouteToProfessionalStatus.Create(
            person: person,
            allRouteTypes: allRoutes,
            routeToProfessionalStatusTypeId: welshrRoute.RouteToProfessionalStatusTypeId,
            status: RouteToProfessionalStatusStatus.Holds,
            holdsFrom: message.AwardedDate,
            trainingStartDate: null,
            trainingEndDate: null,
            trainingSubjectIds: null,
            trainingAgeSpecialismType: null,
            trainingAgeSpecialismRangeFrom: null,
            trainingAgeSpecialismRangeTo: null,
            trainingCountryId: null,
            trainingProviderId: null,
            degreeTypeId: null,
            isExemptFromInduction: null,
            createdBy: DataStore.Postgres.Models.SystemUser.SystemUserId,
            now: clock.UtcNow,
            changeReason: null,
            changeReasonDetail: null,
            evidenceFile: null,
            out var addEvent));

        await dbContext.SaveChangesAsync();
    }
}
