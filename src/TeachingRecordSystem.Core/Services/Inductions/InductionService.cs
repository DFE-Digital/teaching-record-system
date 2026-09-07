using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.Inductions;

public class InductionService(TrsDbContext dbContext, IEventPublisher eventPublisher)
{
    public async Task<bool> SetInductionStatusAsync(SetInductionStatusOptions options, ProcessContext processContext)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var person = await GetPersonAsync(options.PersonId);
        var oldInduction = EventModels.Induction.FromModel(person);

        if (!person.SetInductionStatus(
                options.Status,
                options.StartDate,
                options.CompletedDate,
                options.ExemptionReasonIds,
                processContext.Now))
        {
            return false;
        }

        await SaveAndPublishAsync(eventScope, person, oldInduction);

        return true;
    }

    public async Task<bool> SetCpdInductionStatusAsync(SetCpdInductionStatusOptions options, ProcessContext processContext)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var person = await GetPersonAsync(options.PersonId);
        var oldInduction = EventModels.Induction.FromModel(person);

        if (!person.SetCpdInductionStatus(
                options.Status,
                options.StartDate,
                options.CompletedDate,
                options.CpdModifiedOn,
                processContext.Now))
        {
            return false;
        }

        await SaveAndPublishAsync(eventScope, person, oldInduction);

        return true;
    }

    public async Task<bool> TrySetWelshInductionStatusAsync(SetWelshInductionStatusOptions options, ProcessContext processContext)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var person = await GetPersonAsync(options.PersonId);
        var oldInduction = EventModels.Induction.FromModel(person);

        if (!person.TrySetWelshInductionStatus(
                options.Passed,
                options.StartDate,
                options.CompletedDate,
                processContext.Now))
        {
            return false;
        }

        await SaveAndPublishAsync(eventScope, person, oldInduction);

        return true;
    }

    private async Task<Person> GetPersonAsync(Guid personId) =>
        await dbContext.Persons
            .Include(p => p.Qualifications)
            .SingleOrDefaultAsync(p => p.PersonId == personId)
            ?? throw new NotFoundException(personId, nameof(Person));

    private async Task SaveAndPublishAsync(IEventScope eventScope, Person person, EventModels.Induction oldInduction)
    {
        await dbContext.SaveChangesAsync();

        var induction = EventModels.Induction.FromModel(person);

        await eventScope.PublishEventAsync(
            new PersonInductionUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Induction = induction,
                OldInduction = oldInduction,
                Changes = PersonInductionUpdatedEvent.GetChanges(induction, oldInduction)
            });
    }
}
