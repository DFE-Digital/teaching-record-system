using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public class CreatePersonRouteToProfessionalStatusBuilder
    {
        private Guid? _routeToProfessionalStatusTypeId;
        private RouteToProfessionalStatusStatus _status;
        private DateOnly? _holdsFrom;
        private DateOnly? _trainingStartDate;
        private DateOnly? _trainingEndDate;
        private Guid[] _trainingSubjectIds = [];
        private TrainingAgeSpecialismType? _trainingAgeSpecialismType;
        private int? _trainingAgeSpecialismRangeFrom;
        private int? _trainingAgeSpecialismRangeTo;
        private string? _trainingCountryId;
        private Guid? _trainingProviderId;
        private Guid? _degreeTypeId;
        private bool? _exemptFromInduction;
        private EventModels.RaisedByUserInfo? _createdByUser;
        private string? _changeReason;
        private string? _changeReasonDetail;
        private EventModels.File? _evidenceFile;
        private string? _sourceApplicationReference;

        internal RouteToProfessionalStatusStatus Status => _status;

        internal DateOnly? HoldsFrom => _holdsFrom;

        internal Guid RouteToProfessionalStatusTypeId => _routeToProfessionalStatusTypeId ??
            throw new InvalidOperationException("RouteToProfessionalStatusTypeId not set.");

        public CreatePersonRouteToProfessionalStatusBuilder WithStatus(RouteToProfessionalStatusStatus status)
        {
            _status = status;
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithHoldsFrom(DateOnly holdsFrom)
        {
            _holdsFrom = holdsFrom;
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithTrainingStartDate(DateOnly trainingStartDate)
        {
            _trainingStartDate = trainingStartDate;
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithTrainingEndDate(DateOnly trainingEndDate)
        {
            _trainingEndDate = trainingEndDate;
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithTrainingSubjectIds(Guid[] trainingSubjectIds)
        {
            _trainingSubjectIds = trainingSubjectIds;
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithTrainingAgeSpecialismType(TrainingAgeSpecialismType trainingAgeSpecialismType)
        {
            _trainingAgeSpecialismType = trainingAgeSpecialismType;
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithTrainingAgeSpecialismRangeFrom(int trainingAgeSpecialismRangeFrom)
        {
            _trainingAgeSpecialismRangeFrom = trainingAgeSpecialismRangeFrom;
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithTrainingAgeSpecialismRangeTo(int trainingAgeSpecialismRangeTo)
        {
            _trainingAgeSpecialismRangeTo = trainingAgeSpecialismRangeTo;
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithRouteType(Guid routeTypeId)
        {
            _routeToProfessionalStatusTypeId = routeTypeId;
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithTrainingCountryId(string trainingCountryId)
        {
            _trainingCountryId = trainingCountryId;
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithTrainingProviderId(Guid trainingProviderId)
        {
            _trainingProviderId = trainingProviderId;
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithDegreeTypeId(Guid degreeTypeId)
        {
            _degreeTypeId = degreeTypeId;
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithInductionExemption(bool? isExempt)
        {
            _exemptFromInduction = isExempt;
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithChangeReason(string reason, string reasonDetail)
        {
            _changeReason = reason;
            _changeReasonDetail = reasonDetail;
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithEvidenceFile(string name)
        {
            _evidenceFile = new EventModels.File()
            {
                FileId = Guid.NewGuid(),
                Name = name
            };
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithCreatedByUser(EventModels.RaisedByUserInfo user)
        {
            _createdByUser = user;
            return this;
        }

        public CreatePersonRouteToProfessionalStatusBuilder WithSourceApplicationReference(string sourceApplicationReference)
        {
            _sourceApplicationReference = sourceApplicationReference;
            return this;
        }

        internal async Task ExecuteAsync(
            CreatePersonBuilder createPersonBuilder,
            Person person,
            TestData testData,
            TrsDbContext dbContext)
        {
            if (_routeToProfessionalStatusTypeId is null)
            {
                throw new InvalidOperationException("RouteToProfessionalStatusId has not been set");
            }

            Debug.Assert(person.Qualifications is not null);

            _createdByUser ??= SystemUser.SystemUserId;

            var allRouteTypes = await testData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync();
            var routeType = allRouteTypes.Single(r => r.RouteToProfessionalStatusTypeId == _routeToProfessionalStatusTypeId.Value);
            var now = testData.TimeProvider.UtcNow;

            var professionalStatus = new RouteToProfessionalStatus()
            {
                QualificationId = Guid.NewGuid(),
                CreatedOn = now,
                UpdatedOn = now,
                PersonId = person.PersonId,
                SourceApplicationUserId = null,
                SourceApplicationReference = _sourceApplicationReference,
                RouteToProfessionalStatusTypeId = _routeToProfessionalStatusTypeId.Value,
                Status = _status,
                HoldsFrom = _holdsFrom,
                DegreeTypeId = _degreeTypeId,
                ExemptFromInduction = _exemptFromInduction,
                TrainingStartDate = _trainingStartDate,
                TrainingEndDate = _trainingEndDate,
                TrainingAgeSpecialismRangeFrom = _trainingAgeSpecialismRangeFrom,
                TrainingAgeSpecialismRangeTo = _trainingAgeSpecialismRangeTo,
                TrainingAgeSpecialismType = _trainingAgeSpecialismType,
                TrainingCountryId = _trainingCountryId,
                TrainingProviderId = _trainingProviderId,
                TrainingSubjectIds = _trainingSubjectIds
            };

            var oldPersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person);
            var oldInduction = EventModels.Induction.FromModel(person);

            var professionalStatusType = routeType.ProfessionalStatusType;
            var allRoutes = person.Qualifications.OfType<RouteToProfessionalStatus>().Append(professionalStatus).ToArray();

            if (professionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            {
                // Mirrors RoutesToProfessionalStatusService: QTS awarded before induction was introduced is exempt.
                professionalStatus.ExemptFromInductionDueToQtsDate =
                    _holdsFrom is DateOnly holdsFrom ? holdsFrom < new DateOnly(2000, 5, 7) : null;

                person.RefreshInductionStatusForQtsProfessionalStatusChanged(now, allRouteTypes, allRoutes);
            }

            var newInduction = EventModels.Induction.FromModel(person);
            person.RefreshProfessionalStatusAttributes(professionalStatusType, allRouteTypes, allRoutes);

            if (_routeToProfessionalStatusTypeId.Value == RouteToProfessionalStatusType.QtlsAndSetMembershipId)
            {
                person.RefreshQtlsStatus(allRoutes);
            }

            var personAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person);

            var createdEvent = new RouteToProfessionalStatusCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                RouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(professionalStatus)
            };

            // Mirrors RoutesToProfessionalStatusService: the person's attributes and their induction go on the
            // process as their own events.
            var personAttributesChanges = PersonProfessionalStatusAttributesUpdatedEventChanges.None |
                (personAttributes.QtsDate != oldPersonAttributes.QtsDate ? PersonProfessionalStatusAttributesUpdatedEventChanges.QtsDate : 0) |
                (personAttributes.EytsDate != oldPersonAttributes.EytsDate ? PersonProfessionalStatusAttributesUpdatedEventChanges.EytsDate : 0) |
                (personAttributes.HasEyps != oldPersonAttributes.HasEyps ? PersonProfessionalStatusAttributesUpdatedEventChanges.HasEyps : 0) |
                (personAttributes.PqtsDate != oldPersonAttributes.PqtsDate ? PersonProfessionalStatusAttributesUpdatedEventChanges.PqtsDate : 0) |
                (personAttributes.QtlsStatus != oldPersonAttributes.QtlsStatus ? PersonProfessionalStatusAttributesUpdatedEventChanges.QtlsStatus : 0);

            var inductionChanges = PersonInductionUpdatedEventChanges.None |
                (newInduction.Status != oldInduction.Status ? PersonInductionUpdatedEventChanges.InductionStatus : 0) |
                (newInduction.StatusWithoutExemption != oldInduction.StatusWithoutExemption ? PersonInductionUpdatedEventChanges.InductionStatusWithoutExemption : 0) |
                (newInduction.StartDate != oldInduction.StartDate ? PersonInductionUpdatedEventChanges.InductionStartDate : 0) |
                (newInduction.CompletedDate != oldInduction.CompletedDate ? PersonInductionUpdatedEventChanges.InductionCompletedDate : 0);

            List<IEvent> events = [createdEvent];

            if (personAttributesChanges != PersonProfessionalStatusAttributesUpdatedEventChanges.None)
            {
                events.Add(new PersonProfessionalStatusAttributesUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    PersonId = person.PersonId,
                    PersonAttributes = personAttributes,
                    OldPersonAttributes = oldPersonAttributes,
                    Changes = personAttributesChanges
                });
            }

            if (inductionChanges != PersonInductionUpdatedEventChanges.None)
            {
                events.Add(new PersonInductionUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    PersonId = person.PersonId,
                    Induction = newInduction,
                    OldInduction = oldInduction,
                    Changes = inductionChanges
                });
            }

            dbContext.RouteToProfessionalStatuses.Add(professionalStatus);

            var processId = Guid.NewGuid();

            dbContext.Processes.Add(new Process
            {
                ProcessId = processId,
                ProcessType = ProcessType.RouteToProfessionalStatusCreating,
                CreatedOn = now,
                UpdatedOn = now,
                UserId = _createdByUser.UserId,
                DqtUserId = _createdByUser.DqtUserId,
                DqtUserName = _createdByUser.DqtUserName,
                PersonIds = [person.PersonId],
                OneLoginUserSubjects = [],
                SupportTaskReferences = [],
                ChangeReason = _changeReason is null && _changeReasonDetail is null && _evidenceFile is null ?
                    null :
                    new ChangeReasonWithDetailsAndEvidence
                    {
                        Reason = _changeReason,
                        Details = _changeReasonDetail,
                        EvidenceFile = _evidenceFile,
                        AdditionalInformation = null
                    }
            });

            foreach (var @event in events)
            {
                dbContext.Set<ProcessEvent>().Add(new ProcessEvent
                {
                    ProcessEventId = @event.EventId,
                    ProcessId = processId,
                    EventName = @event.GetType().Name,
                    Payload = @event,
                    PersonIds = [person.PersonId],
                    OneLoginUserSubjects = [],
                    SupportTaskReferences = [],
                    CreatedOn = now
                });
            }
        }
    }
}
