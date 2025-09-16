using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;

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
            _emailAddress = Option.Some(emailAddress);
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

        public async Task<SupportTask> ExecuteAsync(TestData testData)
        {
            var dateOfBirth = _dateOfBirth.ValueOr(testData.GenerateDateOfBirth);
            var evidenceFileId = _evidenceFileId.ValueOr(Guid.NewGuid);
            var evidenceFileName = _evidenceFileName.ValueOr("evidence-file.jpg");
            var emailAddress = _emailAddress.ValueOr(testData.GenerateUniqueEmail);
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

            return await testData.WithDbContextAsync(async dbContext =>
            {
                dbContext.SupportTasks.Add(supportTask);
                dbContext.AddEvent(createdEvent);
                await dbContext.SaveChangesAsync();

                return supportTask;
            });

        }
    }
}
