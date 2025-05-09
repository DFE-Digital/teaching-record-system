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
        var route = await dbContext.RoutesToProfessionalStatus.SingleAsync(p => p.RouteToProfessionalStatusId == RouteToProfessionalStatus.WelshRId);

        dbContext.ProfessionalStatuses.Add(ProfessionalStatus.Create(
            person: person,
            allRoutes: allRoutes,
            routeToProfessionalStatusId: RouteToProfessionalStatus.WelshRId,
            status: ProfessionalStatusStatus.Awarded,
            awardedDate: message.AwardedDate,
            trainingStartDate: message.TrainingStartDate,
            trainingEndDate: message.TrainingEndDate,
            trainingSubjectIds: message.Subjects.ToArray(),
            trainingAgeSpecialismType: message.TrainingAgeSpecialismType,
            trainingAgeSpecialismRangeFrom: message.TrainingAgeSpecialismRangeFrom,
            trainingAgeSpecialismRangeTo: message.TrainingAgeSpecialismRangeTo,
            trainingCountryId: message.TrainingCountryId,
            trainingProviderId: message.TrainingProviderId,
            degreeTypeId: null,
            isExemptFromInduction: null,
            createdBy: DataStore.Postgres.Models.SystemUser.SystemUserId,
            now: clock.UtcNow,
            out var addEvent));

        await dbContext.SaveChangesAsync();

    }
}
