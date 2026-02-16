using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.EventHandlers;

public class CreateLegacySupportTaskEvents(TrsDbContext dbContext) :
    IEventHandler<SupportTaskCreatedEvent>,
    IEventHandler<SupportTaskUpdatedEvent>
{
    public async Task HandleEventAsync(SupportTaskCreatedEvent @event, ProcessContext processContext, IEventScope eventScope)
    {
        var legacyEvent = new LegacyEvents.SupportTaskCreatedEvent
        {
            EventId = @event.EventId,
            CreatedUtc = processContext.Now,
            RaisedBy = processContext.Process.UserId!,
            SupportTask = @event.SupportTask
        };

        dbContext.AddEventWithoutBroadcast(legacyEvent);

        await dbContext.SaveChangesAsync();
    }

    public async Task HandleEventAsync(SupportTaskUpdatedEvent @event, ProcessContext processContext, IEventScope eventScope)
    {
        if (processContext.ProcessType is ProcessType.ApiTrnRequestResolving)
        {
            var trnRequestUpdatedEvent = processContext.Events.OfType<TrnRequestUpdatedEvent>().Single();
            var personDetailsUpdatedEvent = processContext.Events.OfType<PersonDetailsUpdatedEvent>().SingleOrDefault();

            var resolvedPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == trnRequestUpdatedEvent.TrnRequest.ResolvedPersonId);

            var changes = LegacyEvents.ApiTrnRequestSupportTaskUpdatedEventChanges.Status;
            EventModels.PersonDetails? oldPersonAttributes;

            if (personDetailsUpdatedEvent is { })
            {
                changes |=
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? LegacyEvents.ApiTrnRequestSupportTaskUpdatedEventChanges.PersonFirstName : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? LegacyEvents.ApiTrnRequestSupportTaskUpdatedEventChanges.PersonMiddleName : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? LegacyEvents.ApiTrnRequestSupportTaskUpdatedEventChanges.PersonLastName : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? LegacyEvents.ApiTrnRequestSupportTaskUpdatedEventChanges.PersonDateOfBirth : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? LegacyEvents.ApiTrnRequestSupportTaskUpdatedEventChanges.PersonEmailAddress : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? LegacyEvents.ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? LegacyEvents.ApiTrnRequestSupportTaskUpdatedEventChanges.PersonGender : 0);

                oldPersonAttributes = personDetailsUpdatedEvent.OldPersonDetails;
            }
            else
            {
                oldPersonAttributes = null;
            }

            var legacyEvent = new LegacyEvents.ApiTrnRequestSupportTaskUpdatedEvent()
            {
                PersonId = resolvedPerson.PersonId,
                SupportTask = @event.SupportTask,
                OldSupportTask = @event.OldSupportTask,
                RequestData = trnRequestUpdatedEvent.TrnRequest,
                Changes = changes,
                PersonAttributes = new EventModels.PersonDetails
                {
                    FirstName = resolvedPerson.FirstName,
                    MiddleName = resolvedPerson.MiddleName,
                    LastName = resolvedPerson.LastName,
                    DateOfBirth = resolvedPerson.DateOfBirth,
                    EmailAddress = resolvedPerson.EmailAddress,
                    NationalInsuranceNumber = resolvedPerson.NationalInsuranceNumber,
                    Gender = resolvedPerson.Gender
                },
                OldPersonAttributes = oldPersonAttributes,
                Comments = @event.Comments,
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.UserId
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
        else if (processContext.ProcessType is ProcessType.NpqTrnRequestApproving)
        {
            var trnRequestUpdatedEvent = processContext.Events.OfType<TrnRequestUpdatedEvent>().Single();
            var personDetailsUpdatedEvent = processContext.Events.OfType<PersonDetailsUpdatedEvent>().SingleOrDefault();

            var resolvedPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == trnRequestUpdatedEvent.TrnRequest.ResolvedPersonId);

            var changes = LegacyEvents.NpqTrnRequestSupportTaskResolvedEventChanges.Status;
            EventModels.PersonDetails? oldPersonAttributes;

            if (personDetailsUpdatedEvent is { })
            {
                changes |=
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? LegacyEvents.NpqTrnRequestSupportTaskResolvedEventChanges.PersonFirstName : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? LegacyEvents.NpqTrnRequestSupportTaskResolvedEventChanges.PersonMiddleName : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? LegacyEvents.NpqTrnRequestSupportTaskResolvedEventChanges.PersonLastName : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? LegacyEvents.NpqTrnRequestSupportTaskResolvedEventChanges.PersonDateOfBirth : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? LegacyEvents.NpqTrnRequestSupportTaskResolvedEventChanges.PersonEmailAddress : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? LegacyEvents.NpqTrnRequestSupportTaskResolvedEventChanges.PersonNationalInsuranceNumber : 0) |
                    (personDetailsUpdatedEvent.Changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? LegacyEvents.NpqTrnRequestSupportTaskResolvedEventChanges.PersonGender : 0);

                oldPersonAttributes = personDetailsUpdatedEvent.OldPersonDetails;
            }
            else
            {
                oldPersonAttributes = null;
            }

            var resolvedReason = processContext.Events.OfType<PersonCreatedEvent>().Any() ?
                LegacyEvents.NpqTrnRequestResolvedReason.RecordCreated :
                LegacyEvents.NpqTrnRequestResolvedReason.RecordMerged;

            var legacyEvent = new LegacyEvents.NpqTrnRequestSupportTaskResolvedEvent()
            {
                PersonId = resolvedPerson.PersonId,
                SupportTask = @event.SupportTask,
                OldSupportTask = @event.OldSupportTask,
                RequestData = trnRequestUpdatedEvent.TrnRequest,
                Changes = changes,
                PersonAttributes = new EventModels.PersonDetails
                {
                    FirstName = resolvedPerson.FirstName,
                    MiddleName = resolvedPerson.MiddleName,
                    LastName = resolvedPerson.LastName,
                    DateOfBirth = resolvedPerson.DateOfBirth,
                    EmailAddress = resolvedPerson.EmailAddress,
                    NationalInsuranceNumber = resolvedPerson.NationalInsuranceNumber,
                    Gender = resolvedPerson.Gender
                },
                OldPersonAttributes = oldPersonAttributes,
                Comments = @event.Comments,
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.UserId,
                ChangeReason = resolvedReason
            };

            dbContext.AddEventWithoutBroadcast(legacyEvent);

            await dbContext.SaveChangesAsync();
        }
        else if (processContext.ProcessType is ProcessType.NpqTrnRequestRejecting)
        {
            var trnRequestUpdatedEvent = processContext.Events.OfType<TrnRequestUpdatedEvent>().Single();

            var @legacyEvent = new LegacyEvents.NpqTrnRequestSupportTaskRejectedEvent
            {
                EventId = @event.EventId,
                CreatedUtc = processContext.Now,
                RaisedBy = processContext.UserId,
                SupportTask = @event.SupportTask,
                OldSupportTask = @event.OldSupportTask,
                RequestData = trnRequestUpdatedEvent.TrnRequest,
                RejectionReason = @event.RejectionReason
            };

            dbContext.AddEventWithoutBroadcast(@legacyEvent);

            await dbContext.SaveChangesAsync();
        }
        else if (processContext.ProcessType is ProcessType.TeacherPensionsDuplicateSupportTaskResolvingWithoutMerge)
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
