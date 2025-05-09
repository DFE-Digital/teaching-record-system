using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

namespace TeachingRecordSystem.Core.Services.DqtOutbox.Handlers;

public class AddWelshRMessageHandler(TrsDbContext dbContext, IClock clock) : IMessageHandler<AddWelshRMessage>
{
    public async Task HandleMessageAsync(AddWelshRMessage message)
    {
        var person = await dbContext.Persons.SingleAsync(p => p.PersonId == message.PersonId);
        var route = await dbContext.RoutesToProfessionalStatus.SingleAsync(p => p.RouteToProfessionalStatusId == RouteToProfessionalStatus.WelshRId);

        dbContext.ProfessionalStatuses.Add(ProfessionalStatus.Create(
            PersonId: message.PersonId,
            RouteToProfessionalStatusId: RouteToProfessionalStatus.WelshRId,
            Status: ProfessionalStatusStatus.Awarded,
            AwardedDate: message.AwardedDate,
            TrainingStartDate: message.TrainingStartDate,
            TrainingEndDate: message.TrainingEndDate,
            TrainingSubjectIds: message.Subjects.ToArray(),
            TrainingAgeSpecialismType: message.TrainingAgeSpecialismType,
            TrainingAgeSpecialismRangeFrom: message.TrainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo: message.TrainingAgeSpecialismRangeTo,
            TrainingCountryId: message.TrainingCountryId,
            TrainingProviderId: message.TrainingProviderId,
            DegreeTypeId: null,
            IsExemptFromInduction: null,
            DataStore.Postgres.Models.SystemUser.SystemUserId,
            clock.UtcNow,
        out var addEvent));

        await dbContext.SaveChangesAsync();

    }
}
