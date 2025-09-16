using Optional;
using Optional.Unsafe;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon;
public partial class TestData
{
    public Task<SupportTask> CreateTeacherPensionsPotentialDuplicateTaskAsync(
       Guid personId,
       Guid userId,
       Action<CreateTeacherPensionsPotentialDuplicateTaskBuilder>? configure = null)
    {
        var builder = new CreateTeacherPensionsPotentialDuplicateTaskBuilder(personId, userId);
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public class CreateTeacherPensionsPotentialDuplicateTaskBuilder(Guid personId, Guid userId)
    {
        private Option<string> _lastName;
        private Option<string> _firstName;
        private Option<string?> _middleName;
        private Option<Gender?> _gender;
        private Option<DateOnly> _dateOfBirth;
        private Option<string?> _nationalInsuranceNumber;
        private Option<string> _requestId;
        private Option<string?> _emailAddress;
        private Option<DateTime?> _createdOn;
        private Option<TrnRequestMatchedPerson[]> _matchedPersons;
        private string? _fileName;
        private long _integrationTransactionId;

        public CreateTeacherPensionsPotentialDuplicateTaskBuilder WithFirstName(string firstName)
        {
            _firstName = Option.Some(firstName);
            return this;
        }

        public CreateTeacherPensionsPotentialDuplicateTaskBuilder WithMiddleName(string? middleName)
        {
            _middleName = Option.Some(middleName);
            return this;
        }

        public CreateTeacherPensionsPotentialDuplicateTaskBuilder WithLastName(string lastName)
        {
            _lastName = Option.Some(lastName);
            return this;
        }

        public CreateTeacherPensionsPotentialDuplicateTaskBuilder WithDateOfBirth(DateOnly dateOfBirth)
        {
            _dateOfBirth = Option.Some(dateOfBirth);
            return this;
        }

        public CreateTeacherPensionsPotentialDuplicateTaskBuilder WithEmailAddress(string? emailAddress)
        {
            _emailAddress = Option.Some(emailAddress);
            return this;
        }

        public CreateTeacherPensionsPotentialDuplicateTaskBuilder WithNationalInsuranceNumber(string? nationalInsuranceNumber)
        {
            _nationalInsuranceNumber = Option.Some(nationalInsuranceNumber);
            return this;
        }

        public CreateTeacherPensionsPotentialDuplicateTaskBuilder WithGender(Gender? gender)
        {
            _gender = Option.Some(gender);
            return this;
        }

        public CreateTeacherPensionsPotentialDuplicateTaskBuilder WithMatchedPersons(params Guid[] personIds)
        {
            _matchedPersons = Option.Some(personIds.Select(id => new TrnRequestMatchedPerson() { PersonId = id }).ToArray());
            return this;
        }

        public CreateTeacherPensionsPotentialDuplicateTaskBuilder WithCreatedOn(DateTime? date)
        {
            _createdOn = Option.Some(date);
            return this;
        }

        public CreateTeacherPensionsPotentialDuplicateTaskBuilder WithSupportTaskData(string fileName, long integrationTransactionId)
        {
            _fileName = fileName;
            _integrationTransactionId = integrationTransactionId;
            return this;
        }

        public async Task<SupportTask> ExecuteAsync(TestData testData)
        {
            var trnRequestId = _requestId.ValueOr(Guid.NewGuid().ToString);
            var emailAddress = _emailAddress.ValueOr(testData.GenerateUniqueEmail);
            var firstName = _firstName.ValueOr(testData.GenerateFirstName);
            var middleName = _middleName.ValueOrDefault();
            var lastName = _lastName.ValueOr(testData.GenerateLastName);
            var dateOfBirth = _dateOfBirth.ValueOr(testData.GenerateDateOfBirth);
            var nationalInsuranceNumber = _nationalInsuranceNumber.ValueOr(testData.GenerateNationalInsuranceNumber);
            var gender = _gender.ValueOr(testData.GenerateGender());
            var createdOn = _createdOn.ValueOr(testData.Clock.UtcNow);
            var matchedPersons = _matchedPersons.ValueOrDefault();
            var integrationTransactionId = _integrationTransactionId;
            var fileName = _fileName ?? string.Empty;

            var trnRequestMetadata = new Core.DataStore.Postgres.Models.TrnRequestMetadata()
            {
                ApplicationUserId = userId,
                RequestId = Guid.NewGuid().ToString(),
                CreatedOn = testData.Clock.UtcNow,
                IdentityVerified = null,
                OneLoginUserSubject = null,
                Name = (new List<string?> { firstName, middleName, lastName }).OfType<string>().ToArray(),
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                EmailAddress = null,
                NationalInsuranceNumber = nationalInsuranceNumber,
                Gender = gender,
                PotentialDuplicate = true,
                Matches = new Core.DataStore.Postgres.Models.TrnRequestMatches()
                {
                    MatchedPersons = matchedPersons

                }
            };
            var supportTask = Core.DataStore.Postgres.Models.SupportTask.Create(
                SupportTaskType.CapitaImportPotentialDuplicate,
                new Core.Models.SupportTaskData.CapitaPotentialDuplicateData()
                {
                    FileName = fileName,
                    IntegrationTransactionId = integrationTransactionId
                },
                personId: personId,
                null,
                null,
                trnRequestMetadata.RequestId,
                userId,
                createdOn!.Value,
                out var supportTaskCreatedEvent);

            return await testData.WithDbContextAsync(async dbContext =>
            {
                dbContext.TrnRequestMetadata.Add(trnRequestMetadata);
                dbContext.SupportTasks.Add(supportTask);
                dbContext.AddEvent(supportTaskCreatedEvent);
                await dbContext.SaveChangesAsync();

                return supportTask;
            });
        }

    }
}

