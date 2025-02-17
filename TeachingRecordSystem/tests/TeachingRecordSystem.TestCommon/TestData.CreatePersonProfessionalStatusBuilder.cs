namespace TeachingRecordSystem.TestCommon;

using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

public partial class TestData
{
    public class CreatePersonProfessionalStatusBuilder
    {
        private Guid? _personId = null;
        private QualificationType _qualificationType;
        //private Guid _routeToProfessionalStatusId;
        private ProfessionalStatusStatus _status;
        private DateOnly? _awardedDate;
        private DateOnly? _trainingStartDate;
        private DateOnly? _trainingEndDate;
        private Guid[] _trainingSubjectIds = [];
        private TrainingAgeSpecialismType? _trainingAgeSpecialismType;
        private int? _trainingAgeSpecialismRangeFrom;
        private int? _trainingAgeSpecialismRangeTo;
        //private string? _trainingCountryId;
        //private Guid? _trainingProviderId;
        private RouteToProfessionalStatus? _routeToProfessionalStatus;
        private Country? _trainingCountry;
        private TrainingProvider? _trainingProvider;
        //private Guid? _inductionExemptionReasonId;
        private InductionExemptionReason? _inductionExemptionReason;

        public CreatePersonProfessionalStatusBuilder WithPersonId(Guid personId)
        {
            if (_personId is not null && _personId != personId)
            {
                throw new InvalidOperationException("WithPersonId has already been set");
            }

            _personId = personId;
            return this;
        }

        public CreatePersonProfessionalStatusBuilder WithQualificationType(QualificationType qualificationType)
        {
            _qualificationType = qualificationType;
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

        public CreatePersonProfessionalStatusBuilder WithRoute(RouteToProfessionalStatus route)
        {
            _routeToProfessionalStatus = route;
            return this;
        }

        public CreatePersonProfessionalStatusBuilder WithTrainingCountry(Country trainingCountry)
        {
            _trainingCountry = trainingCountry;
            return this;
        }

        internal Task<ProfessionalStatus> ExecuteAsync(
            CreatePersonBuilder createPersonBuilder,
            TestData testData,
            TrsDbContext dbContext)
        {
            var personId = createPersonBuilder.PersonId;

            // for referense data, create some nonsense values temporarily for the the reference tables that aren't populated
            _routeToProfessionalStatus = new RouteToProfessionalStatus()
            {
                RouteToProfessionalStatusId = Guid.NewGuid(),
                Name = "RouteToProfessionalStatusName",
                IsActive = true,
                QualificationType = _qualificationType,
            };
            _trainingCountry = new Country()
            {
                CountryId = Guid.NewGuid().ToString(),
                Name = "CountryName"
            };
            _trainingProvider = new TrainingProvider()
            {
                TrainingProviderId = Guid.NewGuid(),
                Ukprn = "12345678",
                Name = "TrainingProviderName",
                IsActive = true,
            };

            return Task.FromResult(new ProfessionalStatus()
            {
                PersonId = personId,
                QualificationId = new Guid(),
                QualificationType = _qualificationType,
                RouteToProfessionalStatusId = _routeToProfessionalStatus!.RouteToProfessionalStatusId,
                Status = _status,
                AwardedDate = _awardedDate,
                TrainingStartDate = _trainingStartDate,
                TrainingEndDate = _trainingEndDate,
                TrainingSubjectIds = _trainingSubjectIds,
                TrainingAgeSpecialismType = _trainingAgeSpecialismType,
                TrainingAgeSpecialismRangeFrom = _trainingAgeSpecialismRangeFrom,
                TrainingAgeSpecialismRangeTo = _trainingAgeSpecialismRangeTo,
                TrainingCountryId = _trainingCountry?.CountryId,
                TrainingProviderId = _trainingProvider?.TrainingProviderId,
                InductionExemptionReasonId = _inductionExemptionReason?.InductionExemptionReasonId,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            });
        }
    }
}
