using Optional;
using Optional.Unsafe;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon;
public partial class TestData
{
    public async Task<SupportTask> CreateTeacherPensionsPotentialDuplicateTaskAsync(
        Guid? applicationUserId = null,
        string fileName = "filename.csv",
        long integrationTransactionId = 1,
        DateTime? createdOn = null,
        Action<CreatePersonBuilder>? configurePerson = null)
    {
        configurePerson ??= p => { };

        if (applicationUserId is null)
        {
            var applicationUser = await CreateApplicationUserAsync();
            applicationUserId = applicationUser.UserId;
        }

        var person = await CreatePersonAsync(x => configurePerson(x));
        var duplicatePerson1 = await CreatePersonAsync(x => x.WithFirstName(person.FirstName).WithLastName(person.LastName).WithNationalInsuranceNumber(person.NationalInsuranceNumber!));
        var user = await CreateUserAsync();
        var supportTask = await CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithMatchedPersons(duplicatePerson1.PersonId);
                s.WithLastName(person.LastName);
                s.WithFirstName(person.FirstName);
                s.WithMiddleName(person.MiddleName);
                s.WithNationalInsuranceNumber(person.NationalInsuranceNumber);
                s.WithGender(person.Gender);
                s.WithDateOfBirth(person.DateOfBirth);
                s.WithSupportTaskData(fileName, integrationTransactionId);
                s.WithCreatedOn((createdOn ?? Clock.UtcNow).ToUniversalTime());
            });

        return supportTask;
    }

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
        private Option<Guid[]> _matchedPersonIds;
        private string? _fileName;
        private long _integrationTransactionId;
        private Option<SupportTaskStatus> _status;

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
            _matchedPersonIds = Option.Some(personIds);
            return this;
        }

        public CreateTeacherPensionsPotentialDuplicateTaskBuilder WithStatus(SupportTaskStatus status)
        {
            _status = Option.Some(status);
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
            var matchedPersons = _matchedPersonIds.ValueOrDefault();
            var integrationTransactionId = _integrationTransactionId;
            var fileName = _fileName ?? string.Empty;

            var trnRequestMetadata = new TrnRequestMetadata()
            {
                ApplicationUserId = userId,
                RequestId = trnRequestId,
                CreatedOn = testData.Clock.UtcNow,
                IdentityVerified = null,
                OneLoginUserSubject = null,
                Name = (new List<string?> { firstName, middleName, lastName }).OfType<string>().ToArray(),
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                EmailAddress = emailAddress,
                NationalInsuranceNumber = nationalInsuranceNumber,
                Gender = gender,
                PotentialDuplicate = true
            };
            var supportTask = SupportTask.Create(
                SupportTaskType.TeacherPensionsPotentialDuplicate,
                new Core.Models.SupportTasks.TeacherPensionsPotentialDuplicateData()
                {
                    FileName = fileName,
                    IntegrationTransactionId = integrationTransactionId
                },
                personId: personId,
                null,
                trnRequestApplicationUserId: userId,
                trnRequestId: trnRequestMetadata.RequestId,
                createdBy: userId,
                now: createdOn!.Value,
                out var supportTaskCreatedEvent);

            supportTask.Status = _status.ValueOr(SupportTaskStatus.Open);

            return await testData.WithDbContextAsync(async dbContext =>
            {
                dbContext.TrnRequestMetadata.Add(trnRequestMetadata);
                dbContext.SupportTasks.Add(supportTask);
                dbContext.AddEventWithoutBroadcast(supportTaskCreatedEvent);
                await dbContext.SaveChangesAsync();

                return supportTask;
            });
        }
    }
}

