using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Models;
using File = TeachingRecordSystem.Core.Events.Models.File;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<EventBase> CreateProfessionalStatusCreatedEventAsync(Action<CreateProfessionalStatusEventBuilder>? configure = null)
    {
        var builder = new CreateProfessionalStatusEventBuilder();
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public class CreateProfessionalStatusEventBuilder()
    {
        public Guid? _eventId;
        public DateTime? _createdUtc;
        public RaisedByUserInfo? _createdByUser;
        Guid? _personId;
        Core.Events.Models.ProfessionalStatus? _professionalStatus;
        Person? _person;
        ProfessionalStatusPersonAttributes? _personAttributes;
        ProfessionalStatusPersonAttributes? _oldPersonAttributes;
        ProfessionalStatusCreatedEventChanges? _changes;

        public CreateProfessionalStatusEventBuilder WithCreatedByUser(EventModels.RaisedByUserInfo createdByUser)
        {
            _createdByUser = createdByUser;
            return this;
        }

        public CreateProfessionalStatusEventBuilder WithCreatedUtc(DateTime? createdUtc)
        {
            _createdUtc = createdUtc;
            return this;
        }
        public CreateProfessionalStatusEventBuilder ForPerson(Person person)
        {
            _person = person;
            _personId = person.PersonId;
            return this;
        }
        public CreateProfessionalStatusEventBuilder WithProfessionalStatus(Core.DataStore.Postgres.Models.ProfessionalStatus professionalStatus)
        {
            _professionalStatus = Core.Events.Models.ProfessionalStatus.FromModel(professionalStatus);
            return this;
        }

        public CreateProfessionalStatusEventBuilder WithPersonAttributes(ProfessionalStatusPersonAttributes personAttributes)
        {
            _personAttributes = personAttributes;
            return this;
        }

        public CreateProfessionalStatusEventBuilder WithOldPersonAttributes(ProfessionalStatusPersonAttributes personAttributes)
        {
            _oldPersonAttributes = personAttributes;
            return this;
        }

        public CreateProfessionalStatusEventBuilder WithChanges(ProfessionalStatusCreatedEventChanges changes)
        {
            _changes = changes;
            return this;
        }

        public async Task<EventBase> ExecuteAsync(TestData testData)
        {
            if (!_createdUtc.HasValue || _createdByUser is null || _person is null || _professionalStatus is null)
            {
                var nullValues = new List<string>();
                if (!_createdUtc.HasValue) nullValues.Add(nameof(_createdUtc));
                if (_createdByUser == null) nullValues.Add(nameof(_createdByUser));
                if (_person == null) nullValues.Add(nameof(_person));
                if (_professionalStatus == null) nullValues.Add(nameof(_professionalStatus));

                throw new InvalidOperationException($"Setup value(s) cannot be null: {string.Join(",", nullValues)}");
            }

            var events = new List<EventBase>();
            var createdEvent = new ProfessionalStatusCreatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = _createdUtc!.Value,
                RaisedBy = _createdByUser!,
                PersonId = _personId!.Value,
                ProfessionalStatus = _professionalStatus,
                OldPersonAttributes = _oldPersonAttributes ?? new ProfessionalStatusPersonAttributes() { EytsDate = null, HasEyps = false, PqtsDate = null, QtsDate = null },
                PersonAttributes = _personAttributes ?? new ProfessionalStatusPersonAttributes() { EytsDate = null, HasEyps = false, PqtsDate = null, QtsDate = null },
                Changes = _changes ?? ProfessionalStatusCreatedEventChanges.None
            };

            await testData.WithDbContextAsync(async dbContext =>
            {
                dbContext.AddEventWithoutBroadcast(createdEvent);
                await dbContext.SaveChangesAsync();
            });

            return createdEvent;
        }
    }

    public Task<EventBase> CreateProfessionalStatusUpdatedEventAsync(Action<UpdateProfessionalStatusEventBuilder>? configure = null)
    {
        var builder = new UpdateProfessionalStatusEventBuilder();
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public class UpdateProfessionalStatusEventBuilder()
    {
        public Guid? _eventId;
        public DateTime? _createdUtc;
        public RaisedByUserInfo? _createdByUser;
        Guid? _personId;
        Core.Events.Models.ProfessionalStatus? _professionalStatus;
        Core.Events.Models.ProfessionalStatus? _oldProfessionalStatus;
        Person? _person;
        ProfessionalStatusPersonAttributes? _personAttributes;
        ProfessionalStatusPersonAttributes? _oldPersonAttributes;
        ProfessionalStatusUpdatedEventChanges? _changes;
        string? _changeReason;
        string? _changeReasonDetail;
        File? _evidenceFile;

        public UpdateProfessionalStatusEventBuilder WithCreatedByUser(EventModels.RaisedByUserInfo createdByUser)
        {
            _createdByUser = createdByUser;
            return this;
        }

        public UpdateProfessionalStatusEventBuilder WithCreatedUtc(DateTime? createdUtc)
        {
            _createdUtc = createdUtc;
            return this;
        }
        public UpdateProfessionalStatusEventBuilder ForPerson(Person person)
        {
            _person = person;
            _personId = person.PersonId;
            return this;
        }
        public UpdateProfessionalStatusEventBuilder WithProfessionalStatus(Core.DataStore.Postgres.Models.ProfessionalStatus professionalStatus)
        {
            _professionalStatus = Core.Events.Models.ProfessionalStatus.FromModel(professionalStatus);
            return this;
        }

        public UpdateProfessionalStatusEventBuilder WithOldProfessionalStatus(Core.Events.Models.ProfessionalStatus oldProfessionalStatus)
        {
            _oldProfessionalStatus = oldProfessionalStatus;
            return this;
        }

        public UpdateProfessionalStatusEventBuilder WithPersonAttributes(ProfessionalStatusPersonAttributes personAttributes)
        {
            _personAttributes = personAttributes;
            return this;
        }

        public UpdateProfessionalStatusEventBuilder WithOldPersonAttributes(ProfessionalStatusPersonAttributes personAttributes)
        {
            _oldPersonAttributes = personAttributes;
            return this;
        }

        public UpdateProfessionalStatusEventBuilder WithChanges(ProfessionalStatusUpdatedEventChanges changes)
        {
            _changes = changes;
            return this;
        }

        public UpdateProfessionalStatusEventBuilder WithChangeReason(string changeReason)
        {
            _changeReason = changeReason;
            return this;
        }

        public UpdateProfessionalStatusEventBuilder WithChangeReasonDetails(string changeReasonDetails)
        {
            _changeReasonDetail = changeReasonDetails;
            return this;
        }

        public async Task<EventBase> ExecuteAsync(TestData testData)
        {
            if (!_createdUtc.HasValue || _createdByUser is null || _person is null || _professionalStatus is null || _oldProfessionalStatus is null)
            {
                var nullValues = new List<string>();
                if (!_createdUtc.HasValue) nullValues.Add(nameof(_createdUtc));
                if (_createdByUser == null) nullValues.Add(nameof(_createdByUser));
                if (_person == null) nullValues.Add(nameof(_person));
                if (_professionalStatus == null) nullValues.Add(nameof(_professionalStatus));

                throw new InvalidOperationException($"Setup value(s) cannot be null: {string.Join(",", nullValues)}");
            }

            var events = new List<EventBase>();
            var updatedEvent = new ProfessionalStatusUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = _createdUtc!.Value,
                RaisedBy = _createdByUser!,
                PersonId = _personId!.Value,
                ProfessionalStatus = _professionalStatus,
                OldProfessionalStatus = _oldProfessionalStatus,
                OldPersonAttributes = _oldPersonAttributes ?? new ProfessionalStatusPersonAttributes() { EytsDate = null, HasEyps = false, PqtsDate = null, QtsDate = null },
                PersonAttributes = _personAttributes ?? new ProfessionalStatusPersonAttributes() { EytsDate = null, HasEyps = false, PqtsDate = null, QtsDate = null },
                Changes = _changes ?? ProfessionalStatusUpdatedEventChanges.None,
                ChangeReason = _changeReason,
                ChangeReasonDetail = _changeReasonDetail,
                EvidenceFile = _evidenceFile
            };

            await testData.WithDbContextAsync(async dbContext =>
            {
                dbContext.AddEventWithoutBroadcast(updatedEvent);
                await dbContext.SaveChangesAsync();
            });

            return updatedEvent;
        }
    }

    public Task<EventBase> CreateProfessionalStatusDeletedEventAsync(Action<DeleteProfessionalStatusEventBuilder>? configure = null)
    {
        var builder = new DeleteProfessionalStatusEventBuilder();
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public class DeleteProfessionalStatusEventBuilder()
    {
        public Guid? _eventId;
        public DateTime? _createdUtc;
        public RaisedByUserInfo? _createdByUser;
        Guid? _personId;
        Core.Events.Models.ProfessionalStatus? _professionalStatus;
        Person? _person;
        ProfessionalStatusPersonAttributes? _personAttributes;
        ProfessionalStatusPersonAttributes? _oldPersonAttributes;
        ProfessionalStatusDeletedEventChanges? _changes;
        string? _changeReason;
        string? _changeReasonDetail;
        File? _evidenceFile;

        public DeleteProfessionalStatusEventBuilder WithCreatedByUser(EventModels.RaisedByUserInfo createdByUser)
        {
            _createdByUser = createdByUser;
            return this;
        }

        public DeleteProfessionalStatusEventBuilder WithCreatedUtc(DateTime? createdUtc)
        {
            _createdUtc = createdUtc;
            return this;
        }
        public DeleteProfessionalStatusEventBuilder ForPerson(Person person)
        {
            _person = person;
            _personId = person.PersonId;
            return this;
        }
        public DeleteProfessionalStatusEventBuilder WithProfessionalStatus(Core.DataStore.Postgres.Models.ProfessionalStatus professionalStatus)
        {
            _professionalStatus = Core.Events.Models.ProfessionalStatus.FromModel(professionalStatus);
            return this;
        }

        public DeleteProfessionalStatusEventBuilder WithPersonAttributes(ProfessionalStatusPersonAttributes personAttributes)
        {
            _personAttributes = personAttributes;
            return this;
        }

        public DeleteProfessionalStatusEventBuilder WithOldPersonAttributes(ProfessionalStatusPersonAttributes personAttributes)
        {
            _oldPersonAttributes = personAttributes;
            return this;
        }

        public DeleteProfessionalStatusEventBuilder WithChanges(ProfessionalStatusDeletedEventChanges changes)
        {
            _changes = changes;
            return this;
        }

        public DeleteProfessionalStatusEventBuilder WithChangeReason(string changeReason)
        {
            _changeReason = changeReason;
            return this;
        }

        public DeleteProfessionalStatusEventBuilder WithChangeReasonDetails(string changeReasonDetails)
        {
            _changeReasonDetail = changeReasonDetails;
            return this;
        }

        public async Task<EventBase> ExecuteAsync(TestData testData)
        {
            if (!_createdUtc.HasValue || _createdByUser is null || _person is null || _professionalStatus is null)
            {
                var nullValues = new List<string>();
                if (!_createdUtc.HasValue) nullValues.Add(nameof(_createdUtc));
                if (_createdByUser == null) nullValues.Add(nameof(_createdByUser));
                if (_person == null) nullValues.Add(nameof(_person));
                if (_professionalStatus == null) nullValues.Add(nameof(_professionalStatus));

                throw new InvalidOperationException($"Setup value(s) cannot be null: {string.Join(",", nullValues)}");
            }

            var events = new List<EventBase>();
            var updatedEvent = new ProfessionalStatusDeletedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = _createdUtc!.Value,
                RaisedBy = _createdByUser!,
                PersonId = _personId!.Value,
                ProfessionalStatus = _professionalStatus,
                OldPersonAttributes = _oldPersonAttributes ?? new ProfessionalStatusPersonAttributes() { EytsDate = null, HasEyps = false, PqtsDate = null, QtsDate = null },
                PersonAttributes = _personAttributes ?? new ProfessionalStatusPersonAttributes() { EytsDate = null, HasEyps = false, PqtsDate = null, QtsDate = null },
                Changes = _changes ?? ProfessionalStatusDeletedEventChanges.None,
                DeletionReason = _changeReason,
                DeletionReasonDetail = _changeReasonDetail,
                EvidenceFile = _evidenceFile
            };

            await testData.WithDbContextAsync(async dbContext =>
            {
                dbContext.AddEventWithoutBroadcast(updatedEvent);
                await dbContext.SaveChangesAsync();
            });

            return updatedEvent;
        }
    }
}
