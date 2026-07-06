using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.MandatoryQualifications;

public class MandatoryQualificationService(
    TrsDbContext dbContext,
    TimeProvider timeProvider,
    IEventPublisher eventPublisher)
{
    public async Task<MandatoryQualification> CreateMandatoryQualificationAsync(
        CreateMandatoryQualificationOptions options,
        ProcessContext processContext)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var now = timeProvider.UtcNow;

        var qualification = new MandatoryQualification
        {
            QualificationId = Guid.NewGuid(),
            CreatedOn = now,
            UpdatedOn = now,
            PersonId = options.PersonId,
            ProviderId = options.ProviderId,
            Specialism = options.Specialism,
            Status = options.Status,
            StartDate = options.StartDate,
            EndDate = options.EndDate
        };

        dbContext.MandatoryQualifications.Add(qualification);
        await dbContext.SaveChangesAsync();

        await eventScope.PublishEventAsync(
            new MandatoryQualificationCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = options.PersonId,
                MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                    qualification,
                    providerNameHint: MandatoryQualificationProvider.GetById(options.ProviderId).Name)
            });

        return qualification;
    }

    public async Task<MandatoryQualificationUpdatedEventChanges> UpdateMandatoryQualificationAsync(
        UpdateMandatoryQualificationOptions options,
        ProcessContext processContext)
    {
        var qualification = await dbContext.MandatoryQualifications.FindOrThrowAsync(options.QualificationId);

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var oldMandatoryQualification = EventModels.MandatoryQualification.FromModel(
            qualification,
            providerNameHint: qualification.ProviderId is Guid oldProviderId ? MandatoryQualificationProvider.GetById(oldProviderId).Name : null);

        options.ProviderId.MatchSome(providerId => qualification.ProviderId = providerId);
        options.Specialism.MatchSome(specialism => qualification.Specialism = specialism);
        options.Status.MatchSome(status => qualification.Status = status);
        options.StartDate.MatchSome(startDate => qualification.StartDate = startDate);
        options.EndDate.MatchSome(endDate => qualification.EndDate = endDate);

        var changes = MandatoryQualificationUpdatedEventChanges.None |
            (qualification.ProviderId != oldMandatoryQualification.Provider?.MandatoryQualificationProviderId ? MandatoryQualificationUpdatedEventChanges.Provider : 0) |
            (qualification.Specialism != oldMandatoryQualification.Specialism ? MandatoryQualificationUpdatedEventChanges.Specialism : 0) |
            (qualification.Status != oldMandatoryQualification.Status ? MandatoryQualificationUpdatedEventChanges.Status : 0) |
            (qualification.StartDate != oldMandatoryQualification.StartDate ? MandatoryQualificationUpdatedEventChanges.StartDate : 0) |
            (qualification.EndDate != oldMandatoryQualification.EndDate ? MandatoryQualificationUpdatedEventChanges.EndDate : 0);

        if (changes == MandatoryQualificationUpdatedEventChanges.None)
        {
            return changes;
        }

        qualification.UpdatedOn = timeProvider.UtcNow;
        await dbContext.SaveChangesAsync();

        await eventScope.PublishEventAsync(
            new MandatoryQualificationUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = qualification.PersonId,
                MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                    qualification,
                    providerNameHint: qualification.ProviderId is Guid newProviderId ? MandatoryQualificationProvider.GetById(newProviderId).Name : null),
                OldMandatoryQualification = oldMandatoryQualification,
                Changes = changes
            });

        return changes;
    }

    public async Task DeleteMandatoryQualificationAsync(
        DeleteMandatoryQualificationOptions options,
        ProcessContext processContext)
    {
        var qualification = await dbContext.MandatoryQualifications.FindOrThrowAsync(options.QualificationId);

        if (qualification.DeletedOn is not null)
        {
            throw new InvalidOperationException("MandatoryQualification is already deleted.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var now = timeProvider.UtcNow;
        qualification.DeletedOn = now;
        qualification.UpdatedOn = now;

        await dbContext.SaveChangesAsync();

        await eventScope.PublishEventAsync(
            new MandatoryQualificationDeletedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = qualification.PersonId,
                MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                    qualification,
                    providerNameHint: qualification.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null)
            });
    }
}
