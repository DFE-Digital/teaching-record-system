using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.Inductions;

public class InductionService(TrsDbContext dbContext, IEventPublisher eventPublisher)
{
    public async Task<bool> SetInductionStatusAsync(SetInductionStatusOptions options, ProcessContext processContext, CancellationToken cancellationToken = default)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var person = await GetPersonAsync(options.PersonId, cancellationToken);
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

        await SaveAndPublishAsync(eventScope, person, oldInduction, cancellationToken);

        return true;
    }

    public async Task<bool> SetCpdInductionStatusAsync(SetCpdInductionStatusOptions options, ProcessContext processContext, CancellationToken cancellationToken = default)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var person = await GetPersonAsync(options.PersonId, cancellationToken);
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

        await SaveAndPublishAsync(eventScope, person, oldInduction, cancellationToken);

        return true;
    }

    public async Task<bool> TrySetWelshInductionStatusAsync(SetWelshInductionStatusOptions options, ProcessContext processContext, CancellationToken cancellationToken = default)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var person = await GetPersonAsync(options.PersonId, cancellationToken);
        var oldInduction = EventModels.Induction.FromModel(person);

        if (!person.TrySetWelshInductionStatus(
                options.Passed,
                options.StartDate,
                options.CompletedDate,
                processContext.Now))
        {
            return false;
        }

        await SaveAndPublishAsync(eventScope, person, oldInduction, cancellationToken);

        return true;
    }

    private async Task<Person> GetPersonAsync(Guid personId, CancellationToken cancellationToken) =>
        await dbContext.Persons
            .Include(p => p.Qualifications)
            .SingleOrDefaultAsync(p => p.PersonId == personId, cancellationToken)
            ?? throw new NotFoundException(personId, nameof(Person));

    private async Task SaveAndPublishAsync(IEventScope eventScope, Person person, EventModels.Induction oldInduction, CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);

        var induction = EventModels.Induction.FromModel(person);

        await eventScope.PublishEventAsync(
            new PersonInductionUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Induction = induction,
                OldInduction = oldInduction,
                Changes = PersonInductionUpdatedEvent.GetChanges(induction, oldInduction)
            },
            cancellationToken);
    }
}
