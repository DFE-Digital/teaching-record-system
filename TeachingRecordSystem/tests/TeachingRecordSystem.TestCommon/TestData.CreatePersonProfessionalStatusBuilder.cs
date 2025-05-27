using TeachingRecordSystem.Core.DataStore.Postgres;
using ProfessionalStatus = TeachingRecordSystem.Core.DataStore.Postgres.Models.ProfessionalStatus;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public class CreatePersonProfessionalStatusBuilder
    {
        private Guid? _personId = null;
        private Guid? _routeToProfessionalStatusId;
        private ProfessionalStatusStatus _status;
        private DateOnly? _awardedDate;
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

        private Guid QualificationId { get; } = Guid.NewGuid();

        public CreatePersonProfessionalStatusBuilder WithPersonId(Guid personId)
        {
            if (_personId is not null && _personId != personId)
            {
                throw new InvalidOperationException("PersonId has already been set");
            }

            _personId = personId;
            return this;
        }

        public CreatePersonProfessionalStatusBuilder WithStatus(ProfessionalStatusStatus status)
        {
            _status = status;
            return this;
        }

        public CreatePersonProfessionalStatusBuilder WithAwardedDate(DateOnly awardedDate)
        {
            _awardedDate = awardedDate;
            return this;
        }

        public CreatePersonProfessionalStatusBuilder WithTrainingStartDate(DateOnly trainingStartDate)
        {
            _trainingStartDate = trainingStartDate;
            return this;
        }

        public CreatePersonProfessionalStatusBuilder WithTrainingEndDate(DateOnly trainingEndDate)
        {
            _trainingEndDate = trainingEndDate;
            return this;
        }

        public CreatePersonProfessionalStatusBuilder WithTrainingSubjectIds(Guid[] trainingSubjectIds)
        {
            _trainingSubjectIds = trainingSubjectIds;
            return this;
        }

        public CreatePersonProfessionalStatusBuilder WithTrainingAgeSpecialismType(TrainingAgeSpecialismType trainingAgeSpecialismType)
        {
            _trainingAgeSpecialismType = trainingAgeSpecialismType;
            return this;
        }

        public CreatePersonProfessionalStatusBuilder WithTrainingAgeSpecialismRangeFrom(int trainingAgeSpecialismRangeFrom)
        {
            _trainingAgeSpecialismRangeFrom = trainingAgeSpecialismRangeFrom;
            return this;
        }

        public CreatePersonProfessionalStatusBuilder WithTrainingAgeSpecialismRangeTo(int trainingAgeSpecialismRangeTo)
        {
            _trainingAgeSpecialismRangeTo = trainingAgeSpecialismRangeTo;
            return this;
        }

        public CreatePersonProfessionalStatusBuilder WithRoute(Guid routeId)
        {
            _routeToProfessionalStatusId = routeId;
            return this;
        }

        public CreatePersonProfessionalStatusBuilder WithTrainingCountryId(string trainingCountryId)
        {
            _trainingCountryId = trainingCountryId;
            return this;
        }

        public CreatePersonProfessionalStatusBuilder WithTrainingProviderId(Guid trainingProviderId)
        {
            _trainingProviderId = trainingProviderId;
            return this;
        }

        public CreatePersonProfessionalStatusBuilder WithDegreeTypeId(Guid degreeTypeId)
        {
            _degreeTypeId = degreeTypeId;
            return this;
        }

        public CreatePersonProfessionalStatusBuilder WithInductionExemption(bool? isExempt)
        {
            _exemptFromInduction = isExempt;
            return this;
        }

        internal Task<Guid> ExecuteAsync(
            CreatePersonBuilder createPersonBuilder,
            TestData testData,
            TrsDbContext dbContext)
        {
            if (_routeToProfessionalStatusId is null)
            {
                throw new InvalidOperationException("RouteToProfessionalStatusId has not been set");
            }

            var personId = createPersonBuilder.PersonId;

            var professionalStatus = new ProfessionalStatus()
            {
                PersonId = personId,
                QualificationId = QualificationId,
                RouteToProfessionalStatusId = _routeToProfessionalStatusId!.Value,
                Status = _status,
                AwardedDate = _awardedDate,
                TrainingStartDate = _trainingStartDate,
                TrainingEndDate = _trainingEndDate,
                TrainingSubjectIds = _trainingSubjectIds,
                TrainingAgeSpecialismType = _trainingAgeSpecialismType,
                TrainingAgeSpecialismRangeFrom = _trainingAgeSpecialismRangeFrom,
                TrainingAgeSpecialismRangeTo = _trainingAgeSpecialismRangeTo,
                TrainingCountryId = _trainingCountryId,
                TrainingProviderId = _trainingProviderId,
                ExemptFromInduction = _exemptFromInduction,
                DegreeTypeId = _degreeTypeId,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };

            dbContext.ProfessionalStatuses.Add(professionalStatus);

            return Task.FromResult(professionalStatus.QualificationId);
        }
    }
}
