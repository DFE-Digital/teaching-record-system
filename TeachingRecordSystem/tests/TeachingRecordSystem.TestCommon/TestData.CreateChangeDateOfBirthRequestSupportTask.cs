using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<SupportTask> CreateChangeDateOfBirthRequestSupportTaskAsync(
       Guid personId,
       Action<CreateChangeDateOfBirthRequestSupportTaskBuilder>? configure = null)
    {
        var builder = new CreateChangeDateOfBirthRequestSupportTaskBuilder(personId);
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public class CreateChangeDateOfBirthRequestSupportTaskBuilder(Guid personId)
    {
        private Option<DateOnly> _dateOfBirth;
        private Option<Guid> _evidenceFileId;
        private Option<string> _evidenceFileName;
        private Option<string> _emailAddress;
        private Option<SupportTaskStatus> _status;
        private Option<DateTime> _createdOn;
        private bool _hasEmailAddress = true;

        public CreateChangeDateOfBirthRequestSupportTaskBuilder WithDateOfBirth(DateOnly dateOfBirth)
        {
            _dateOfBirth = Option.Some(dateOfBirth);
            return this;
        }
        public CreateChangeDateOfBirthRequestSupportTaskBuilder WithEvidenceFileId(Guid evidenceFileId)
        {
            _evidenceFileId = Option.Some(evidenceFileId);
            return this;
        }
        public CreateChangeDateOfBirthRequestSupportTaskBuilder WithEvidenceFileName(string evidenceFileName)
        {
            _evidenceFileName = Option.Some(evidenceFileName);
            return this;
        }
        public CreateChangeDateOfBirthRequestSupportTaskBuilder WithEmailAddress(string emailAddress)
        {
            if (!_hasEmailAddress)
            {
                throw new InvalidOperationException("Cannot specify an email address and also indicate that there is no email address.");
            }
            _emailAddress = Option.Some(emailAddress);
            return this;
        }
        public CreateChangeDateOfBirthRequestSupportTaskBuilder WithoutEmailAddress()
        {
            if (_emailAddress.HasValue)
            {
                throw new InvalidOperationException("Cannot specify an email address and also indicate that there is no email address.");
            }
            _emailAddress = Option.None<string>();
            _hasEmailAddress = false;
            return this;
        }

        public CreateChangeDateOfBirthRequestSupportTaskBuilder WithStatus(SupportTaskStatus status)
        {
            _status = Option.Some(status);
            return this;
        }
        public CreateChangeDateOfBirthRequestSupportTaskBuilder WithCreatedOn(DateTime createdOn)
        {
            _createdOn = Option.Some(createdOn);
            return this;
        }

        public Task<SupportTask> ExecuteAsync(TestData testData)
        {
            var dateOfBirth = _dateOfBirth.ValueOr(testData.GenerateDateOfBirth);
            var evidenceFileId = _evidenceFileId.ValueOr(Guid.NewGuid);
            var evidenceFileName = _evidenceFileName.ValueOr("evidence-file.jpg");
            var emailAddress = _hasEmailAddress ? _emailAddress.ValueOr(testData.GenerateUniqueEmail) : null;
            var status = _status.ValueOr(SupportTaskStatus.Open);
            var createdOn = _createdOn.ValueOr(testData.Clock.UtcNow);

            var data = new ChangeDateOfBirthRequestData()
            {
                DateOfBirth = dateOfBirth,
                EvidenceFileId = evidenceFileId,
                EvidenceFileName = evidenceFileName,
                EmailAddress = emailAddress,
                ChangeRequestOutcome = null
            };

            var supportTask = SupportTask.Create(
                SupportTaskType.ChangeDateOfBirthRequest,
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
