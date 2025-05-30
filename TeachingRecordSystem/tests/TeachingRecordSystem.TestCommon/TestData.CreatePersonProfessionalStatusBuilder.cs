using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
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
        private EventModels.RaisedByUserInfo? _createdByUser;

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

        public CreatePersonProfessionalStatusBuilder WithCreatedByUser(EventModels.RaisedByUserInfo user)
        {
            _createdByUser = user;
            return this;
        }

        internal async Task<(Guid ProfessionalStatusId, IReadOnlyCollection<EventBase> Events)> ExecuteAsync(
            CreatePersonBuilder createPersonBuilder,
            Person person,
            TestData testData,
            TrsDbContext dbContext)
        {
            if (_routeToProfessionalStatusId is null)
            {
                throw new InvalidOperationException("RouteToProfessionalStatusId has not been set");
            }
            if (_createdByUser is null)
            {
                _createdByUser = EventModels.RaisedByUserInfo.FromUserId(Core.DataStore.Postgres.Models.SystemUser.SystemUserId);
            }

            var personId = createPersonBuilder.PersonId;
            var allRoutes = await testData.ReferenceDataCache.GetRoutesToProfessionalStatusAsync();

            var professionalStatus = ProfessionalStatus.Create(
                person,
                allRoutes,
                _routeToProfessionalStatusId!.Value,
                _status,
                _awardedDate,
                _trainingStartDate,
                _trainingEndDate,
                _trainingSubjectIds,
                _trainingAgeSpecialismType,
                _trainingAgeSpecialismRangeFrom,
                _trainingAgeSpecialismRangeTo,
                _trainingCountryId,
                _trainingProviderId,
                _degreeTypeId,
                _exemptFromInduction,
                _createdByUser,
                DateTime.UtcNow,
                out var @createdEvent
                );

            dbContext.ProfessionalStatuses.Add(professionalStatus);
            dbContext.AddEventWithoutBroadcast(createdEvent);

            return (professionalStatus.QualificationId, [createdEvent]);
        }
    }
}
