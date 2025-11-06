using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
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
            _emailAddress = Option.Some(emailAddress);
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
            var emailAddress = _emailAddress.ValueOr(testData.GenerateUniqueEmail);
            var status = _status.ValueOr(SupportTaskStatus.Open);
            var createdOn = _createdOn.ValueOr(testData.Clock.UtcNow);

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
