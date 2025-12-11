using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<SupportTask> CreateChangeNameRequestSupportTaskAsync(
       Action<CreateChangeNameRequestSupportTaskBuilder>? configure = null,
       Action<CreatePersonBuilder>? configurePerson = null)
    {
        var person = await CreatePersonAsync(configurePerson);

        configure ??= b => { };
        configure = b => configure(b.WithLastName(GenerateChangedLastName(person.LastName)));

        return await CreateChangeNameRequestSupportTaskAsync(person.PersonId, configure);
    }

    public Task<SupportTask> CreateChangeNameRequestSupportTaskAsync(
        Guid personId,
        Action<CreateChangeNameRequestSupportTaskBuilder>? configure = null)
    {
        var builder = new CreateChangeNameRequestSupportTaskBuilder(personId);
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public class CreateChangeNameRequestSupportTaskBuilder(Guid personId)
    {
        private Option<string> _firstName;
        private Option<string> _middleName;
        private Option<string> _lastName;
        private Option<Guid> _evidenceFileId;
        private Option<string> _evidenceFileName;
        private Option<string> _emailAddress;
        private Option<SupportTaskStatus> _status;
        private Option<DateTime> _createdOn;
        private bool _hasEmailAddress = true;

        public CreateChangeNameRequestSupportTaskBuilder WithFirstName(string firstName)
        {
            _firstName = Option.Some(firstName);
            return this;
        }

        public CreateChangeNameRequestSupportTaskBuilder WithMiddleName(string middleName)
        {
            _middleName = Option.Some(middleName);
            return this;
        }

        public CreateChangeNameRequestSupportTaskBuilder WithLastName(string lastName)
        {
            _lastName = Option.Some(lastName);
            return this;
        }

        public CreateChangeNameRequestSupportTaskBuilder WithEvidenceFileId(Guid evidenceFileId)
        {
            _evidenceFileId = Option.Some(evidenceFileId);
            return this;
        }

        public CreateChangeNameRequestSupportTaskBuilder WithEvidenceFileName(string evidenceFileName)
        {
            _evidenceFileName = Option.Some(evidenceFileName);
            return this;
        }

        public CreateChangeNameRequestSupportTaskBuilder WithEmailAddress(string emailAddress)
        {
            if (!_hasEmailAddress)
            {
                throw new InvalidOperationException("Cannot specify an email address and also indicate that there is no email address.");
            }
            _emailAddress = Option.Some(emailAddress);
            return this;
        }

        public CreateChangeNameRequestSupportTaskBuilder WithoutEmailAddress()
        {
            if (_emailAddress.HasValue)
            {
                throw new InvalidOperationException("Cannot specify an email address and also indicate that there is no email address.");
            }
            _hasEmailAddress = false;
            _emailAddress = Option.None<string>();
            return this;
        }

        public CreateChangeNameRequestSupportTaskBuilder WithStatus(SupportTaskStatus status)
        {
            _status = Option.Some(status);
            return this;
        }

        public CreateChangeNameRequestSupportTaskBuilder WithCreatedOn(DateTime createdOn)
        {
            _createdOn = Option.Some(createdOn);
            return this;
        }

        public Task<SupportTask> ExecuteAsync(TestData testData)
        {
            var firstName = _firstName.ValueOr(testData.GenerateFirstName);
            var middleName = _middleName.ValueOr(testData.GenerateMiddleName);
            var lastName = _lastName.ValueOr(testData.GenerateLastName);
            var evidenceFileId = _evidenceFileId.ValueOr(Guid.NewGuid);
            var evidenceFileName = _evidenceFileName.ValueOr("evidence-file.jpg");
            var emailAddress = _hasEmailAddress ? _emailAddress.ValueOr(testData.GenerateUniqueEmail) : null;
            var status = _status.ValueOr(SupportTaskStatus.Open);
            var createdOn = _createdOn.ValueOr(testData.Clock.UtcNow).ToUniversalTime();

            var data = new ChangeNameRequestData
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName,
                EmailAddress = emailAddress,
                ChangeRequestOutcome = null
            };

            var supportTask = SupportTask.Create(
                SupportTaskType.ChangeNameRequest,
                data,
                personId: personId,
                oneLoginUserSubject: null,
                trnRequestApplicationUserId: null,
                trnRequestId: null,
                createdBy: SystemUser.SystemUserId,
                createdOn,
                out var createdEvent);
            supportTask.Status = status;

            return testData.WithDbContextAsync(async dbContext =>
            {
                dbContext.SupportTasks.Add(supportTask);
                dbContext.AddEventWithoutBroadcast(createdEvent);
                await dbContext.SaveChangesAsync();

                return supportTask;
            });
        }
    }
}
