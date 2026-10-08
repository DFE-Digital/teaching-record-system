using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.EventHandlers;

public class CreateLegacySupportTaskEvents(TrsDbContext dbContext) :
    IEventHandler<SupportTaskUpdatedEvent>
{
    public async Task HandleEventAsync(SupportTaskUpdatedEvent @event, ProcessContext processContext, IEventScope eventScope)
    {
        if (processContext.ProcessType is ProcessType.TeacherPensionsDuplicateSupportTaskResolvingWithoutMerge)
        {
            var trnRequestUpdatedEvent = processContext.Events.OfType<TrnRequestUpdatedEvent>().Single();

            var resolvedPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == trnRequestUpdatedEvent.TrnRequest.ResolvedPersonId);

            var legacyEvent = new LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent
            {
                PersonId = resolvedPerson.PersonId,
                RequestData = trnRequestUpdatedEvent.TrnRequest,
                ChangeReason = LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedReason.RecordKept,
                Changes = LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.Status,
                PersonAttributes = EventModels.PersonDetails.FromModel(resolvedPerson),
                OldPersonAttributes = null,
                Comments = @event.Comments,
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.UserId,
                SupportTask = @event.SupportTask,
                OldSupportTask = @event.OldSupportTask
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
        else if (processContext.ProcessType is ProcessType.TeacherPensionsDuplicateSupportTaskResolvingWithMerge)
        {
            var trnRequestUpdatedEvent = processContext.Events.OfType<TrnRequestUpdatedEvent>().Single();
            var personDetailsUpdatedEvent = processContext.Events.OfType<PersonDetailsUpdatedEvent>().SingleOrDefault();

            var resolvedPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == trnRequestUpdatedEvent.TrnRequest.ResolvedPersonId);

            var changes = LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.Status;
            var personAttributes = EventModels.PersonDetails.FromModel(resolvedPerson);
            EventModels.PersonDetails? oldPersonAttributes;

            if (personDetailsUpdatedEvent is not null)
            {
                changes |=
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonFirstName : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonMiddleName : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonLastName : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonDateOfBirth : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonNationalInsuranceNumber : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.PersonGender : 0);

                oldPersonAttributes = personDetailsUpdatedEvent.OldPersonDetails;
            }
            else
            {
                oldPersonAttributes = personAttributes;
            }

            var legacyEvent = new LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent
            {
                PersonId = resolvedPerson.PersonId,
                RequestData = trnRequestUpdatedEvent.TrnRequest,
                ChangeReason = LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedReason.RecordMerged,
                Changes = changes,
                PersonAttributes = personAttributes,
                OldPersonAttributes = oldPersonAttributes,
                Comments = @event.Comments,
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.UserId,
                SupportTask = @event.SupportTask,
                OldSupportTask = @event.OldSupportTask
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
    }
}
