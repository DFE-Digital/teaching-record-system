using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;

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
            var personAttributesUpdated = person.RefreshProfessionalStatusAttributes(professionalStatusType, allRouteTypes, allRoutes);
            var qtlsStatusUpdated = _routeToProfessionalStatusTypeId.Value == RouteToProfessionalStatusType.QtlsAndSetMembershipId &&
                person.RefreshQtlsStatus(allRoutes);

            var changes = RouteToProfessionalStatusCreatedEventChanges.None |
                (professionalStatusType is ProfessionalStatusType.QualifiedTeacherStatus && personAttributesUpdated
                    ? RouteToProfessionalStatusCreatedEventChanges.PersonQtsDate
                    : 0) |
                (professionalStatusType is ProfessionalStatusType.EarlyYearsTeacherStatus && personAttributesUpdated
                    ? RouteToProfessionalStatusCreatedEventChanges.PersonEytsDate
                    : 0) |
                (professionalStatusType is ProfessionalStatusType.EarlyYearsProfessionalStatus && personAttributesUpdated
                    ? RouteToProfessionalStatusCreatedEventChanges.PersonHasEyps
                    : 0) |
                (professionalStatusType is ProfessionalStatusType.PartialQualifiedTeacherStatus && personAttributesUpdated
                    ? RouteToProfessionalStatusCreatedEventChanges.PersonPqtsDate
                    : 0) |
                (newInduction.Status != oldInduction.Status
                    ? RouteToProfessionalStatusCreatedEventChanges.PersonInductionStatus
                    : 0) |
                (newInduction.StatusWithoutExemption != oldInduction.StatusWithoutExemption
                    ? RouteToProfessionalStatusCreatedEventChanges.PersonInductionStatusWithoutExemption
                    : 0) |
                (qtlsStatusUpdated ? RouteToProfessionalStatusCreatedEventChanges.PersonQtlsStatus : 0);

            var createdEvent = new RouteToProfessionalStatusCreatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                PersonId = person.PersonId,
                RaisedBy = _createdByUser,
                RouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(professionalStatus),
                PersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person),
                OldPersonAttributes = oldPersonAttributes,
                ChangeReason = _changeReason,
                ChangeReasonDetail = _changeReasonDetail,
                EvidenceFile = _evidenceFile,
                Changes = changes,
                Induction = newInduction,
                OldInduction = oldInduction,
                AdditionalInformation = null
            };

            dbContext.RouteToProfessionalStatuses.Add(professionalStatus);
            dbContext.AddEventWithoutBroadcast(createdEvent);
        }
    }
}
