using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;

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

        internal async Task<Guid> ExecuteAsync(
            CreatePersonBuilder createPersonBuilder,
            Person person,
            TestData testData,
            TrsDbContext dbContext)
        {
            if (_routeToProfessionalStatusTypeId is null)
            {
                throw new InvalidOperationException("RouteToProfessionalStatusId has not been set");
            }

            _createdByUser ??= SystemUser.SystemUserId;

            var routesToProfessionalStatusService = new RoutesToProfessionalStatusService(
                dbContext,
                testData.ReferenceDataCache,
                testData.TimeProvider);

            var professionalStatus = await routesToProfessionalStatusService.CreateRouteToProfessionalStatusAsync(
                new CreateRouteToProfessionalStatusOptions
                {
                    PersonId = person.PersonId,
                    RouteToProfessionalStatusTypeId = _routeToProfessionalStatusTypeId!.Value,
                    Status = _status,
                    CreatedBy = _createdByUser,
                    SourceApplicationReference = _sourceApplicationReference,
                    HoldsFrom = _holdsFrom,
                    TrainingStartDate = _trainingStartDate,
                    TrainingEndDate = _trainingEndDate,
                    TrainingSubjectIds = _trainingSubjectIds,
                    TrainingAgeSpecialismType = _trainingAgeSpecialismType,
                    TrainingAgeSpecialismRangeFrom = _trainingAgeSpecialismRangeFrom,
                    TrainingAgeSpecialismRangeTo = _trainingAgeSpecialismRangeTo,
                    TrainingCountryId = _trainingCountryId,
                    TrainingProviderId = _trainingProviderId,
                    DegreeTypeId = _degreeTypeId,
                    IsExemptFromInduction = _exemptFromInduction,
                    ChangeReason = _changeReason,
                    ChangeReasonDetail = _changeReasonDetail,
                    EvidenceFile = _evidenceFile
                });

            return professionalStatus.QualificationId;
        }
    }
}
